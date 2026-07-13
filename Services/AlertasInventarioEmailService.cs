using BLL_ConstruccionAPI.Data;
using BLL_ConstruccionAPI.Helpers;
using BLL_ConstruccionAPI.Repositories.Interfaces;
using BLL_ConstruccionAPI.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace BLL_ConstruccionAPI.Services;

public class AlertasInventarioEmailService : BackgroundService
{
    private readonly IServiceProvider _services;
    private readonly ILogger<AlertasInventarioEmailService> _logger;
    private readonly IConfiguration _config;

    public AlertasInventarioEmailService(
        IServiceProvider services,
        ILogger<AlertasInventarioEmailService> logger,
        IConfiguration config)
    {
        _services = services;
        _logger = logger;
        _config = config;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var intervalHours = _config.GetValue<int>("AlertasInventario:IntervalHoras", 4);
        var interval = TimeSpan.FromHours(intervalHours);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await EnviarAlertasSiNecesarioAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al enviar alertas de inventario");
            }

            await Task.Delay(interval, stoppingToken);
        }
    }

    private async Task EnviarAlertasSiNecesarioAsync()
    {
        using var scope = _services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var emailService = scope.ServiceProvider.GetRequiredService<IEmailService>();
        var usuarioRepo = scope.ServiceProvider.GetRequiredService<IUsuarioRepository>();

        var materialesBajos = await context.AlmacenCentral
            .Include(a => a.Material)
            .Where(a => a.Material != null && a.Stock <= a.Material.StockMinimo)
            .ToListAsync();

        if (!materialesBajos.Any()) return;

        var alertas = materialesBajos.Select(a => (
            Material: a.Material!.Nombre,
            Stock: a.Stock,
            StockMinimo: a.Material.StockMinimo,
            Severidad: a.Stock == 0 ? "Crítico" : "Bajo"
        )).ToList();

        var notificables = await usuarioRepo.GetUsuariosNotificablesAsync();
        foreach (var usuario in notificables)
        {
            foreach (var correo in usuario.CorreosNotificacion())
                await emailService.SendAlertasInventarioAsync(correo, usuario.Nombre, alertas);
        }

        _logger.LogInformation("Alertas de inventario enviadas: {Count} materiales bajo mínimo", alertas.Count);
    }
}
