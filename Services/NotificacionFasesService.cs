using BLL_ConstruccionAPI.Data;
using BLL_ConstruccionAPI.Models.Auth;
using BLL_ConstruccionAPI.Models.Enums;
using BLL_ConstruccionAPI.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace BLL_ConstruccionAPI.Services;

public class NotificacionFasesService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<NotificacionFasesService> _logger;

    private static readonly int[] RolesDestinatarios = [1, 3];
    private const int HoraMinima = 7; // No enviar antes de las 7am hora México

    private static readonly TimeZoneInfo ZonaMexico = ObtenerZonaMexico();

    public NotificacionFasesService(
        IServiceScopeFactory scopeFactory,
        ILogger<NotificacionFasesService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Servicio de notificación de fases iniciado.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await VerificarYEnviarAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al procesar notificaciones de fases.");
            }

            await Task.Delay(TimeSpan.FromHours(1), stoppingToken);
        }
    }

    private async Task VerificarYEnviarAsync()
    {
        var ahora = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, ZonaMexico);

        if (ahora.Hour < HoraMinima) return;

        var hoy = ahora.Date;
        var manana = hoy.AddDays(1);

        using var scope = _scopeFactory.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var emailService = scope.ServiceProvider.GetRequiredService<IEmailService>();

        await ProcesarTipoAsync(context, emailService, hoy, manana, esHoy: false);
        await ProcesarTipoAsync(context, emailService, hoy, manana, esHoy: true);
        await ProcesarFasesAtrasadasAsync(context, emailService, hoy);
    }

    private async Task ProcesarTipoAsync(
        AppDbContext context,
        IEmailService emailService,
        DateTime hoy,
        DateTime manana,
        bool esHoy)
    {
        var tipo = esHoy ? "hoy" : "mañana";
        var fechaObjetivo = esHoy ? hoy : manana;

        var yaEnviado = await context.RegistrosNotificacionFase
            .AnyAsync(r => r.Fecha.Date == hoy.Date && r.Tipo == tipo);

        if (yaEnviado) return;

        var fases = await context.FaseProyectos
            .Include(f => f.Proyecto)
            .Where(f =>
                f.FechaLimite.Date == fechaObjetivo.Date &&
                f.Estado != EstadoFase.Completada &&
                f.Proyecto != null &&
                f.Proyecto.Activo)
            .OrderBy(f => f.Proyecto!.Nombre)
            .ThenBy(f => f.Orden)
            .Select(f => new
            {
                ProyectoNombre = f.Proyecto!.Nombre,
                FaseNombre = f.Nombre,
                f.Descripcion,
                f.FechaLimite
            })
            .ToListAsync();

        if (fases.Count == 0)
        {
            _logger.LogInformation(
                "Notificación fases ({Tipo}): sin fases que vencen {Fecha:dd/MM/yyyy}.",
                tipo, fechaObjetivo);
            return;
        }

        var destinatarios = await context.Usuarios
            .Where(u => RolesDestinatarios.Contains(u.RolId) && u.Activo)
            .ToListAsync();

        if (destinatarios.Count == 0)
        {
            _logger.LogWarning("Notificación fases ({Tipo}): no hay destinatarios activos.", tipo);
            return;
        }

        var listaFases = fases
            .Select(f => (f.ProyectoNombre, f.FaseNombre, f.Descripcion, f.FechaLimite))
            .ToList();

        foreach (var usuario in destinatarios)
        {
            var enviado = await emailService.SendNotificacionFasesVencenAsync(
                usuario.Email,
                usuario.Nombre,
                esHoy,
                listaFases);

            if (!enviado)
                _logger.LogWarning(
                    "Notificación fases ({Tipo}): no se pudo enviar a {Email}.",
                    tipo, usuario.Email);
        }

        context.RegistrosNotificacionFase.Add(new RegistroNotificacionFase
        {
            Fecha = hoy,
            Tipo = tipo,
            FechaEnvio = DateTime.UtcNow
        });
        await context.SaveChangesAsync();

        _logger.LogInformation(
            "Notificación fases ({Tipo}) enviada: {Fases} fases a {Destinatarios} usuarios.",
            tipo, fases.Count, destinatarios.Count);
    }

    private async Task ProcesarFasesAtrasadasAsync(
        AppDbContext context,
        IEmailService emailService,
        DateTime hoy)
    {
        var yaEnviado = await context.RegistrosNotificacionFase
            .AnyAsync(r => r.Fecha.Date == hoy.Date && r.Tipo == "atrasadas");

        if (yaEnviado) return;

        var fases = await context.FaseProyectos
            .Include(f => f.Proyecto)
            .Where(f =>
                f.FechaLimite.Date < hoy.Date &&
                f.Estado != EstadoFase.Completada &&
                f.Proyecto != null &&
                f.Proyecto.Activo)
            .OrderBy(f => f.Proyecto!.Nombre)
            .ThenBy(f => f.Orden)
            .Select(f => new
            {
                ProyectoNombre = f.Proyecto!.Nombre,
                FaseNombre = f.Nombre,
                f.Descripcion,
                f.FechaLimite
            })
            .ToListAsync();

        if (fases.Count == 0)
        {
            _logger.LogInformation("Notificación fases atrasadas: sin fases atrasadas al {Fecha:dd/MM/yyyy}.", hoy);
            return;
        }

        var destinatarios = await context.Usuarios
            .Where(u => RolesDestinatarios.Contains(u.RolId) && u.Activo)
            .ToListAsync();

        if (destinatarios.Count == 0)
        {
            _logger.LogWarning("Notificación fases atrasadas: no hay destinatarios activos.");
            return;
        }

        var listaFases = fases
            .Select(f => (f.ProyectoNombre, f.FaseNombre, f.Descripcion, f.FechaLimite,
                DiasAtraso: (int)(hoy - f.FechaLimite).TotalDays))
            .ToList();

        foreach (var usuario in destinatarios)
        {
            var enviado = await emailService.SendNotificacionFasesAtrasadasAsync(
                usuario.Email,
                usuario.Nombre,
                listaFases);

            if (!enviado)
                _logger.LogWarning("Notificación fases atrasadas: no se pudo enviar a {Email}.", usuario.Email);
        }

        context.RegistrosNotificacionFase.Add(new RegistroNotificacionFase
        {
            Fecha = hoy,
            Tipo = "atrasadas",
            FechaEnvio = DateTime.UtcNow
        });
        await context.SaveChangesAsync();

        _logger.LogInformation(
            "Notificación fases atrasadas enviada: {Fases} fases en {Proyectos} proyectos a {Destinatarios} usuarios.",
            fases.Count,
            fases.Select(f => f.ProyectoNombre).Distinct().Count(),
            destinatarios.Count);
    }

    private static TimeZoneInfo ObtenerZonaMexico()
    {
        try { return TimeZoneInfo.FindSystemTimeZoneById("America/Mexico_City"); }
        catch { return TimeZoneInfo.FindSystemTimeZoneById("Central Standard Time"); }
    }
}
