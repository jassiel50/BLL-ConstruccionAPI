using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using BLL_ConstruccionAPI.Models.Auth;
using Microsoft.IdentityModel.Tokens;

namespace BLL_ConstruccionAPI.Helpers;

public class MfaTicketHelper
{
    private readonly IConfiguration _config;

    public MfaTicketHelper(IConfiguration config)
    {
        _config = config;
    }

    /// <summary>
    /// Genera un ticket JWT temporal para el flujo MFA (válido 10 minutos)
    /// </summary>
    public string GenerarMfaTicket(MfaTicket ticket)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:SecretKey"]!));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var expira = DateTime.UtcNow.AddMinutes(10);

        var claims = new[]
        {
            new Claim("mfa_ticket", "true"),
            new Claim("usuario_id", ticket.UsuarioId.ToString()),
            new Claim("email", ticket.Email),
            new Claim("nombre_usuario", ticket.NombreUsuario),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var token = new JwtSecurityToken(
            issuer: _config["Jwt:Issuer"],
            audience: _config["Jwt:Audience"],
            claims: claims,
            expires: expira,
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    /// <summary>
    /// Valida y decodifica un MFA ticket
    /// </summary>
    public (bool Valid, MfaTicket? Ticket) ValidarMfaTicket(string token)
    {
        try
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(_config["Jwt:SecretKey"]!);

            var validationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = _config["Jwt:Issuer"],
                ValidAudience = _config["Jwt:Audience"],
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ClockSkew = TimeSpan.Zero
            };

            var principal = tokenHandler.ValidateToken(token, validationParameters, out _);

            if (principal.FindFirst("mfa_ticket")?.Value != "true")
                return (false, null);

            var usuarioId = int.Parse(principal.FindFirst("usuario_id")!.Value);
            var email = principal.FindFirst("email")!.Value;
            var nombreUsuario = principal.FindFirst("nombre_usuario")!.Value;

            var ticket = new MfaTicket
            {
                UsuarioId = usuarioId,
                Email = email,
                NombreUsuario = nombreUsuario
            };

            return (true, ticket);
        }
        catch
        {
            return (false, null);
        }
    }
}
