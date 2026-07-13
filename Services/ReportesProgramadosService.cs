using BLL_ConstruccionAPI.Data;
using BLL_ConstruccionAPI.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace BLL_ConstruccionAPI.Services;

public class ReportesProgramadosService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<ReportesProgramadosService> _logger;

    private static readonly TimeZoneInfo ZonaMexico = ObtenerZonaMexico();

    public ReportesProgramadosService(
        IServiceScopeFactory scopeFactory,
        ILogger<ReportesProgramadosService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Servicio de reportes programados personalizados iniciado.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcesarReportesPendientesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al procesar reportes programados.");
            }

            await Task.Delay(TimeSpan.FromMinutes(15), stoppingToken);
        }
    }

    private async Task ProcesarReportesPendientesAsync()
    {
        var ahoraLocal = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, ZonaMexico);

        using var scope = _scopeFactory.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var reporteService = scope.ServiceProvider.GetRequiredService<ConfiguracionReporteService>();

        var configuraciones = await context.ConfiguracionesReporte
            .Include(c => c.Usuario)
            .Where(c => c.Activo && c.Usuario.Activo)
            .ToListAsync();

        foreach (var config in configuraciones)
        {
            if (!CorrespondeEnviar(config.Frecuencia, config.HoraEnvio, ahoraLocal, config.UltimoEnvio))
                continue;

            var destinatarios = await reporteService.ResolverDestinatariosAsync(config, config.Usuario);

            try
            {
                await reporteService.EnviarReportesDeConfiguracionAsync(
                    config, destinatarios, config.Usuario.Nombre);

                config.UltimoEnvio = DateTime.UtcNow;
                await context.SaveChangesAsync();

                _logger.LogInformation(
                    "Reporte '{Nombre}' enviado a {Destinatarios}.",
                    config.Nombre, string.Join(", ", destinatarios));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Error al enviar reporte '{Nombre}' a {Destinatarios}.",
                    config.Nombre, string.Join(", ", destinatarios));
            }
        }
    }

    private static bool CorrespondeEnviar(
        string frecuencia, int horaEnvio, DateTime ahoraLocal, DateTime? ultimoEnvio)
    {
        // Ya pasó (o es) la hora programada de hoy. Se usa ">=" en vez de "==" para que,
        // si el servicio estuvo caído durante la hora exacta (reinicio, redeploy, etc.),
        // se recupere y envíe en cuanto vuelva a correr — en vez de saltarse el día completo.
        if (ahoraLocal.Hour < horaEnvio) return false;

        // Ya se envió hoy — evita reenvíos aunque el proceso se reinicie varias veces el mismo día.
        if (ultimoEnvio.HasValue)
        {
            var ultimoLocal = TimeZoneInfo.ConvertTimeFromUtc(ultimoEnvio.Value, ZonaMexico);
            if (ultimoLocal.Date == ahoraLocal.Date) return false;
        }

        return frecuencia switch
        {
            "diario" => true,
            "semanal" => ahoraLocal.DayOfWeek == DayOfWeek.Monday,
            "mensual" => ahoraLocal.Day == 1,
            _ => false
        };
    }

    private static TimeZoneInfo ObtenerZonaMexico()
    {
        try { return TimeZoneInfo.FindSystemTimeZoneById("America/Mexico_City"); }
        catch { return TimeZoneInfo.FindSystemTimeZoneById("Central Standard Time"); }
    }
}
