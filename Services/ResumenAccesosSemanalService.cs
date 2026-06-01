using BLL_ConstruccionAPI.Data;
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

    private DateOnly _ultimoEnvio = DateOnly.MinValue;

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
                var ahora = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, ZonaMexico);

                if (ahora.DayOfWeek == DayOfWeek.Monday && ahora.Hour == 13)
                {
                    var hoy = DateOnly.FromDateTime(ahora);
                    if (_ultimoEnvio != hoy)
                    {
                        await EnviarResumenAsync(ahora);
                        _ultimoEnvio = hoy;
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al enviar resumen semanal de accesos.");
            }

            await Task.Delay(TimeSpan.FromHours(1), stoppingToken);
        }
    }

    private async Task EnviarResumenAsync(DateTime ahoraLocal)
    {
        using var scope = _scopeFactory.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var emailService = scope.ServiceProvider.GetRequiredService<IEmailService>();

        // Semana anterior: lunes pasado 00:00 → domingo 23:59:59 (hora local)
        var lunesActual = ahoraLocal.Date; // hoy ES lunes
        var inicioLocal = lunesActual.AddDays(-7);
        var finLocal = lunesActual.AddDays(-1).Add(new TimeSpan(23, 59, 59));

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
            .Select(a => (a.Nombre, a.NombreUsuario, a.TotalAccesos,
                TimeZoneInfo.ConvertTimeFromUtc(a.UltimoAcceso, ZonaMexico)))
            .ToList();

        foreach (var admin in destinatarios)
        {
            var enviado = await emailService.SendResumenAccesosSemanalAsync(
                admin.Email,
                admin.Nombre,
                inicioLocal,
                finLocal.Date,
                listaAccesos);

            if (!enviado)
                _logger.LogWarning("Resumen semanal: no se pudo enviar a {Email}.", admin.Email);
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
