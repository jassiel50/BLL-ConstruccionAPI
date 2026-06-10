namespace BLL_ConstruccionAPI.Services.Interfaces;

public interface IEmailService
{
    Task<bool> SendMfaCodeAsync(string toEmail, string userName, string code, int expirationMinutes);
    Task<bool> SendPasswordResetCodeAsync(string toEmail, string userName, string code, int expirationMinutes);
    Task<bool> SendProyectoIniciadoAdminAsync(
        string toEmail,
        string adminName,
        string proyectoNombre,
        string clienteNombre,
        string ubicacion,
        DateTime fechaInicio,
        DateTime? fechaFin,
        decimal montoContrato,
        decimal presupuestoEstimado,
        string numeroCotizacion,
        string ordenCompra,
        List<(string Nombre, string Descripcion, DateTime FechaLimite)> fases);

    Task<bool> SendAlertasInventarioAsync(
        string toEmail,
        string adminName,
        List<(string Material, decimal Stock, decimal StockMinimo, string Severidad)> alertas);

    Task<bool> SendReporteProgramadoAsync(
        string toEmail,
        string adminName,
        string tipoReporte,
        byte[] pdfBytes,
        string nombreArchivo);

    Task<bool> SendResumenAccesosSemanalAsync(
        string toEmail,
        string adminName,
        DateTime semanaInicio,
        DateTime semanaFin,
        List<(string Nombre, string NombreUsuario, int TotalAccesos, DateTime UltimoAcceso)> accesos);

    Task<bool> SendNotificacionFasesVencenAsync(
        string toEmail,
        string adminName,
        bool esHoy,
        List<(string ProyectoNombre, string FaseNombre, string Descripcion, DateTime FechaLimite)> fases);

    Task<bool> SendNotificacionFasesAtrasadasAsync(
        string toEmail,
        string adminName,
        List<(string ProyectoNombre, string FaseNombre, string Descripcion, DateTime FechaLimite, int DiasAtraso)> fases);
}
