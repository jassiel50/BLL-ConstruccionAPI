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
