using BLL_ConstruccionAPI.Repositories.Interfaces;
using BLL_ConstruccionAPI.Services.Interfaces;

namespace BLL_ConstruccionAPI.Services;

public class ReportesAutomaticosService : BackgroundService
{
    private readonly IServiceProvider _services;
    private readonly ILogger<ReportesAutomaticosService> _logger;
    private readonly IConfiguration _config;

    public ReportesAutomaticosService(
        IServiceProvider services,
        ILogger<ReportesAutomaticosService> logger,
        IConfiguration config)
    {
        _services = services;
        _logger = logger;
        _config = config;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // Esperar hasta la siguiente hora de envío configurada antes del primer ciclo
        await EsperarProximaEjecucionAsync(stoppingToken);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await EnviarReportesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al enviar reportes automáticos");
            }

            // Esperar 24 horas hasta el siguiente ciclo
            await Task.Delay(TimeSpan.FromHours(24), stoppingToken);
        }
    }

    private async Task EsperarProximaEjecucionAsync(CancellationToken stoppingToken)
    {
        var horaEnvio = _config.GetValue<int>("ReportesAutomaticos:HoraEnvio", 8);
        var now = DateTime.UtcNow;
        var proximaEjecucion = DateTime.UtcNow.Date.AddHours(horaEnvio);
        if (proximaEjecucion <= now)
            proximaEjecucion = proximaEjecucion.AddDays(1);

        var demora = proximaEjecucion - now;
        _logger.LogInformation("Próximo reporte automático en {Demora}", demora);
        await Task.Delay(demora, stoppingToken);
    }

    private async Task EnviarReportesAsync()
    {
        if (!_config.GetValue<bool>("ReportesAutomaticos:Habilitado", false))
            return;

        using var scope = _services.CreateScope();
        var reportesService = scope.ServiceProvider.GetRequiredService<IReportesService>();
        var emailService = scope.ServiceProvider.GetRequiredService<IEmailService>();
        var usuarioRepo = scope.ServiceProvider.GetRequiredService<IUsuarioRepository>();

        var notificables = await usuarioRepo.GetUsuariosNotificablesAsync();
        if (!notificables.Any()) return;

        var hoy = DateTime.UtcNow;
        var hace30 = hoy.AddDays(-30);

        var reportes = new List<(string Tipo, Func<Task<byte[]>> Generador, string NombreArchivo)>
        {
            ("Inventario", () => reportesService.GenerarInventarioAsync(),
                $"Inventario_{hoy:yyyyMMdd}.pdf"),
            ("Proyectos", () => reportesService.GenerarProyectosAsync(),
                $"Proyectos_{hoy:yyyyMMdd}.pdf"),
            ("Movimientos últimos 30 días", () => reportesService.GenerarMovimientosAsync(hace30, hoy),
                $"Movimientos_{hoy:yyyyMMdd}.pdf"),
        };

        foreach (var (tipo, generador, nombreArchivo) in reportes)
        {
            try
            {
                var pdf = await generador();
                foreach (var usuario in notificables)
                    await emailService.SendReporteProgramadoAsync(usuario.Email, usuario.Nombre, tipo, pdf, nombreArchivo);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generando reporte {Tipo}", tipo);
            }
        }

        _logger.LogInformation("Reportes automáticos enviados a {Count} usuarios", notificables.Count);
    }
}
