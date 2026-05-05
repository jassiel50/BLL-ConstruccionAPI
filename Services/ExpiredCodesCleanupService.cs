using BLL_ConstruccionAPI.Data;
using Microsoft.EntityFrameworkCore;

namespace BLL_ConstruccionAPI.Services;

/// <summary>
/// Servicio en segundo plano que elimina códigos MFA y de reset de contraseña
/// expirados o usados. Se ejecuta cada hora.
/// </summary>
public class ExpiredCodesCleanupService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<ExpiredCodesCleanupService> _logger;
    private readonly TimeSpan _intervalo = TimeSpan.FromHours(1);

    public ExpiredCodesCleanupService(
        IServiceScopeFactory scopeFactory,
        ILogger<ExpiredCodesCleanupService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Servicio de limpieza de códigos expirados iniciado.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await LimpiarCodigosExpiradosAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error durante la limpieza de códigos expirados.");
            }

            await Task.Delay(_intervalo, stoppingToken);
        }
    }

    private async Task LimpiarCodigosExpiradosAsync()
    {
        using var scope = _scopeFactory.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var ahora = DateTime.UtcNow;
        var corte = ahora.AddDays(-1); // Mantener registros del último día para auditoría

        var mfaEliminados = await context.MfaEmailCodes
            .Where(c => c.Usado || c.FechaExpira < corte)
            .ExecuteDeleteAsync();

        var resetEliminados = await context.PasswordResetCodes
            .Where(c => c.Usado || c.FechaExpira < corte)
            .ExecuteDeleteAsync();

        if (mfaEliminados > 0 || resetEliminados > 0)
        {
            _logger.LogInformation(
                "Limpieza completada: {Mfa} códigos MFA y {Reset} códigos de reset eliminados.",
                mfaEliminados, resetEliminados);
        }
    }
}
