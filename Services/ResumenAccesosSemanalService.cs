using BLL_ConstruccionAPI.Data;
using BLL_ConstruccionAPI.Helpers;
using BLL_ConstruccionAPI.Models.Auth;
using BLL_ConstruccionAPI.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace BLL_ConstruccionAPI.Services;

public class ResumenAccesosSemanalService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<ResumenAccesosSemanalService> _logger;

    // Roles que reciben el correo: 1 = Admin, 3 = Sistemas
    private static readonly int[] RolesDestinatarios = [1, 3];

    private static readonly TimeZoneInfo ZonaMexico = ObtenerZonaMexico();

    public ResumenAccesosSemanalService(
        IServiceScopeFactory scopeFactory,
        ILogger<ResumenAccesosSemanalService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Servicio de resumen semanal de accesos iniciado.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await VerificarYEnviarAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al procesar resumen semanal de accesos.");
            }

            await Task.Delay(TimeSpan.FromHours(1), stoppingToken);
        }
    }

    private async Task VerificarYEnviarAsync()
    {
        var ahora = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, ZonaMexico);
        var lunesRef = ObtenerLunesDeReferencia(ahora);

        // Si aún no llegó la hora del primer envío posible, no hacer nada
        if (lunesRef == null) return;

        using var scope = _scopeFactory.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var yaEnviado = await context.RegistrosCorreoSemanal
            .AnyAsync(r => r.FechaLunes.Date == lunesRef.Value.Date);

        if (yaEnviado) return;

        _logger.LogInformation(
            "Correo semanal pendiente para la semana del {Lunes:dd/MM/yyyy}. Enviando ahora...",
            lunesRef.Value);

        await EnviarResumenAsync(context, lunesRef.Value,
            scope.ServiceProvider.GetRequiredService<IEmailService>());

        context.RegistrosCorreoSemanal.Add(new RegistroCorreoSemanal
        {
            FechaLunes = lunesRef.Value,
            FechaEnvio = DateTime.UtcNow
        });
        await context.SaveChangesAsync();
    }

    // Devuelve el lunes más reciente para el que ya debería haberse enviado el correo
    // (lunes pasado o este lunes si ya son >= 13:00). Null si aún no corresponde.
    private static DateTime? ObtenerLunesDeReferencia(DateTime ahoraLocal)
    {
        // Días desde el lunes (0 = lunes, 6 = domingo)
        var diasDesdeElLunes = ((int)ahoraLocal.DayOfWeek + 6) % 7;
        var lunesActual = ahoraLocal.Date.AddDays(-diasDesdeElLunes);

        // Es lunes pero antes de la 1pm → el envío de hoy aún no toca; referencia es la semana pasada
        if (diasDesdeElLunes == 0 && ahoraLocal.Hour < 13)
        {
            var lunesPasado = lunesActual.AddDays(-7);
            // Si es la primera semana de vida del sistema, no hay semana previa que enviar
            return lunesPasado;
        }

        return lunesActual;
    }

    private async Task EnviarResumenAsync(AppDbContext context, DateTime lunesRef, IEmailService emailService)
    {
        var inicioLocal = lunesRef.Date.AddDays(-7); // lunes de la semana anterior
        var finLocal = lunesRef.Date.AddDays(-1).Add(new TimeSpan(23, 59, 59)); // domingo anterior

        var inicioUtc = TimeZoneInfo.ConvertTimeToUtc(inicioLocal, ZonaMexico);
        var finUtc = TimeZoneInfo.ConvertTimeToUtc(finLocal, ZonaMexico);

        var accesos = await context.LogAccesos
            .Where(l => l.Exitoso && l.Fecha >= inicioUtc && l.Fecha <= finUtc)
            .Join(context.Usuarios,
                l => l.UsuarioId,
                u => u.Id,
                (l, u) => new { u.Nombre, u.NombreUsuario, l.Fecha })
            .GroupBy(x => new { x.NombreUsuario, x.Nombre })
            .Select(g => new
            {
                g.Key.Nombre,
                g.Key.NombreUsuario,
                TotalAccesos = g.Count(),
                UltimoAcceso = g.Max(x => x.Fecha)
            })
            .OrderByDescending(x => x.TotalAccesos)
            .ToListAsync();

        var destinatarios = await context.Usuarios
            .Where(u => RolesDestinatarios.Contains(u.RolId) && u.Activo)
            .ToListAsync();

        if (destinatarios.Count == 0)
        {
            _logger.LogWarning("Resumen semanal: no se encontraron administradores activos.");
            return;
        }

        var listaAccesos = accesos
            .Select(a => (
                a.Nombre,
                a.NombreUsuario,
                a.TotalAccesos,
                TimeZoneInfo.ConvertTimeFromUtc(a.UltimoAcceso, ZonaMexico)))
            .ToList();

        foreach (var admin in destinatarios)
        {
            foreach (var correo in admin.CorreosNotificacion())
            {
                var enviado = await emailService.SendResumenAccesosSemanalAsync(
                    correo,
                    admin.Nombre,
                    inicioLocal,
                    finLocal.Date,
                    listaAccesos);

                if (!enviado)
                    _logger.LogWarning("Resumen semanal: no se pudo enviar a {Email}.", correo);
            }
        }

        _logger.LogInformation(
            "Resumen semanal enviado a {Count} administradores. Período: {Inicio:dd/MM/yyyy} - {Fin:dd/MM/yyyy}. Usuarios con acceso: {Usuarios}.",
            destinatarios.Count, inicioLocal, finLocal.Date, accesos.Count);
    }

    private static TimeZoneInfo ObtenerZonaMexico()
    {
        try { return TimeZoneInfo.FindSystemTimeZoneById("America/Mexico_City"); }
        catch { return TimeZoneInfo.FindSystemTimeZoneById("Central Standard Time"); }
    }
}
