using BLL_ConstruccionAPI.Services.Interfaces;
using System.Net.Http;
using System.Text;
using System.Text.Json;

namespace BLL_ConstruccionAPI.Services;

public class ResendEmailService : IEmailService
{
    private readonly IConfiguration _config;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<ResendEmailService> _logger;

    public ResendEmailService(
        IConfiguration config, 
        IHttpClientFactory httpClientFactory,
        ILogger<ResendEmailService> logger)
    {
        _config = config;
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    public async Task<bool> SendMfaCodeAsync(string toEmail, string userName, string code, int expirationMinutes)
    {
        var subject = "Código de verificación MFA - BLL Construcción";
        var htmlBody = $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset=""utf-8"">
    <style>
        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
        .header {{ background-color: #0066cc; color: white; padding: 20px; text-align: center; }}
        .content {{ background-color: #f9f9f9; padding: 30px; margin: 20px 0; }}
        .code-box {{ background-color: #ffffff; border: 2px solid #0066cc; padding: 20px; text-align: center; font-size: 32px; font-weight: bold; letter-spacing: 5px; margin: 20px 0; }}
        .footer {{ text-align: center; color: #666; font-size: 12px; margin-top: 20px; }}
    </style>
</head>
<body>
    <div class=""container"">
        <div class=""header"">
            <h1>BLL Construcción</h1>
        </div>
        <div class=""content"">
            <h2>Hola {userName},</h2>
            <p>Has solicitado un código de verificación para acceder a tu cuenta.</p>
            <p>Tu código de verificación es:</p>
            <div class=""code-box"">{code}</div>
            <p><strong>Este código expirará en {expirationMinutes} minutos.</strong></p>
            <p>Si no solicitaste este código, por favor ignora este correo y contacta al administrador del sistema.</p>
        </div>
        <div class=""footer"">
            <p>© 2026 BLL Construcción. Todos los derechos reservados.</p>
            <p>Este es un correo automático, por favor no respondas a este mensaje.</p>
        </div>
    </div>
</body>
</html>";

        return await SendEmailAsync(toEmail, subject, htmlBody);
    }

    public async Task<bool> SendPasswordResetCodeAsync(string toEmail, string userName, string code, int expirationMinutes)
    {
        var subject = "Código de recuperación de contraseña - BLL Construcción";
        var htmlBody = $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset=""utf-8"">
    <style>
        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
        .header {{ background-color: #dc3545; color: white; padding: 20px; text-align: center; }}
        .content {{ background-color: #f9f9f9; padding: 30px; margin: 20px 0; }}
        .code-box {{ background-color: #ffffff; border: 2px solid #dc3545; padding: 20px; text-align: center; font-size: 32px; font-weight: bold; letter-spacing: 5px; margin: 20px 0; }}
        .warning {{ background-color: #fff3cd; border-left: 4px solid #ffc107; padding: 10px; margin: 20px 0; }}
        .footer {{ text-align: center; color: #666; font-size: 12px; margin-top: 20px; }}
    </style>
</head>
<body>
    <div class=""container"">
        <div class=""header"">
            <h1>Recuperación de Contraseña</h1>
        </div>
        <div class=""content"">
            <h2>Hola {userName},</h2>
            <p>Recibimos una solicitud para restablecer tu contraseña.</p>
            <p>Tu código de recuperación es:</p>
            <div class=""code-box"">{code}</div>
            <p><strong>Este código expirará en {expirationMinutes} minutos.</strong></p>
            <div class=""warning"">
                <strong>⚠️ Importante:</strong> Si no solicitaste restablecer tu contraseña, por favor ignora este correo y contacta inmediatamente al administrador del sistema.
            </div>
        </div>
        <div class=""footer"">
            <p>© 2026 BLL Construcción. Todos los derechos reservados.</p>
            <p>Este es un correo automático, por favor no respondas a este mensaje.</p>
        </div>
    </div>
</body>
</html>";

        return await SendEmailAsync(toEmail, subject, htmlBody);
    }

    public async Task<bool> SendProyectoIniciadoAdminAsync(
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
        List<(string Nombre, string Descripcion, DateTime FechaLimite)> fases)
    {
        var subject = $"Nuevo proyecto iniciado: {proyectoNombre} - BLL Construcción";

        var fasesHtml = new System.Text.StringBuilder();
        foreach (var fase in fases)
        {
            fasesHtml.Append($@"
                <tr>
                    <td style=""padding: 8px; border: 1px solid #ddd;"">{fase.Nombre}</td>
                    <td style=""padding: 8px; border: 1px solid #ddd;"">{fase.Descripcion}</td>
                    <td style=""padding: 8px; border: 1px solid #ddd;"">{fase.FechaLimite:dd/MM/yyyy}</td>
                </tr>");
        }

        var fechaFinTexto = fechaFin.HasValue ? fechaFin.Value.ToString("dd/MM/yyyy") : "Por definir";

        var htmlBody = $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset=""utf-8"">
    <style>
        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
        .container {{ max-width: 700px; margin: 0 auto; padding: 20px; }}
        .header {{ background-color: #1a3a5c; color: white; padding: 20px; text-align: center; }}
        .content {{ background-color: #f9f9f9; padding: 30px; margin: 20px 0; }}
        .info-table {{ width: 100%; border-collapse: collapse; margin: 15px 0; }}
        .info-table td {{ padding: 8px 12px; border-bottom: 1px solid #e0e0e0; }}
        .info-table td:first-child {{ font-weight: bold; width: 40%; color: #1a3a5c; }}
        .fases-table {{ width: 100%; border-collapse: collapse; margin: 15px 0; }}
        .fases-table th {{ background-color: #1a3a5c; color: white; padding: 10px; text-align: left; border: 1px solid #ddd; }}
        .fases-table td {{ padding: 8px; border: 1px solid #ddd; }}
        .fases-table tr:nth-child(even) {{ background-color: #f2f2f2; }}
        .monto-box {{ background-color: #e8f0fe; border-left: 4px solid #1a3a5c; padding: 15px; margin: 15px 0; }}
        .footer {{ text-align: center; color: #666; font-size: 12px; margin-top: 20px; }}
        h3 {{ color: #1a3a5c; border-bottom: 2px solid #1a3a5c; padding-bottom: 5px; }}
    </style>
</head>
<body>
    <div class=""container"">
        <div class=""header"">
            <h1>BLL Construcción</h1>
            <h2>Nuevo Proyecto Iniciado</h2>
        </div>
        <div class=""content"">
            <p>Hola <strong>{adminName}</strong>,</p>
            <p>Se ha registrado el inicio de un nuevo proyecto. A continuación los detalles:</p>

            <h3>Información General</h3>
            <table class=""info-table"">
                <tr><td>Proyecto:</td><td>{proyectoNombre}</td></tr>
                <tr><td>Cliente:</td><td>{clienteNombre}</td></tr>
                <tr><td>Ubicación:</td><td>{ubicacion}</td></tr>
                <tr><td>Fecha de inicio:</td><td>{fechaInicio:dd/MM/yyyy}</td></tr>
                <tr><td>Fecha de fin:</td><td>{fechaFinTexto}</td></tr>
                <tr><td>N° Cotización:</td><td>{numeroCotizacion}</td></tr>
                <tr><td>Orden de compra:</td><td>{ordenCompra}</td></tr>
            </table>

            <h3>Información Financiera</h3>
            <div class=""monto-box"">
                <table class=""info-table"">
                    <tr><td>Monto contrato:</td><td>{montoContrato:C2}</td></tr>
                    <tr><td>Presupuesto estimado:</td><td>{presupuestoEstimado:C2}</td></tr>
                </table>
            </div>

            <h3>Fases del Proyecto</h3>
            <table class=""fases-table"">
                <thead>
                    <tr>
                        <th>Nombre</th>
                        <th>Descripción</th>
                        <th>Fecha Límite</th>
                    </tr>
                </thead>
                <tbody>
                    {fasesHtml}
                </tbody>
            </table>
        </div>
        <div class=""footer"">
            <p>© 2026 BLL Construcción. Todos los derechos reservados.</p>
            <p>Este es un correo automático, por favor no respondas a este mensaje.</p>
        </div>
    </div>
</body>
</html>";

        return await SendEmailAsync(toEmail, subject, htmlBody);
    }

    private async Task<bool> SendEmailAsync(string toEmail, string subject, string htmlBody)
    {
        try
        {
            var apiKey = _config["Resend:ApiKey"];
            var fromEmail = _config["Resend:FromEmail"];
            var fromName = _config["Resend:FromName"];

            if (string.IsNullOrWhiteSpace(apiKey))
            {
                _logger.LogError("Resend:ApiKey no está configurada");
                return false;
            }

            var client = _httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");

            var payload = new
            {
                from = $"{fromName} <{fromEmail}>",
                to = new[] { toEmail },
                subject = subject,
                html = htmlBody
            };

            var content = new StringContent(
                JsonSerializer.Serialize(payload),
                Encoding.UTF8,
                "application/json");

            var response = await client.PostAsync("https://api.resend.com/emails", content);

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                _logger.LogError("Error al enviar email con Resend: {StatusCode} - {Error}", 
                    response.StatusCode, error);
                return false;
            }

            _logger.LogInformation("Email enviado exitosamente a {Email}", toEmail);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Excepción al enviar email a {Email}", toEmail);
            return false;
        }
    }
}
