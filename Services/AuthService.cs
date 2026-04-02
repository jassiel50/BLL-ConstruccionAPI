using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using BLL_ConstruccionAPI.DTOs.Auth;
using BLL_ConstruccionAPI.Models.Auth;
using BLL_ConstruccionAPI.Repositories.Interfaces;
using BLL_ConstruccionAPI.Services.Interfaces;
using Microsoft.IdentityModel.Tokens;
using OtpNet;

namespace BLL_ConstruccionAPI.Services;

public class AuthService : IAuthService
{
    private readonly IUsuarioRepository _usuarioRepo;
    private readonly IConfiguration _config;

    public AuthService(IUsuarioRepository usuarioRepo, IConfiguration config)
    {
        _usuarioRepo = usuarioRepo;
        _config = config;
    }

    // ─── REGISTER ────────────────────────────────────────────────────────────
    public async Task<(bool Success, string Message)> RegisterAsync(RegisterRequestDto dto, string ipOrigen)
    {
        if (await _usuarioRepo.ExisteNombreUsuarioAsync(dto.NombreUsuario))
            return (false, "El nombre de usuario ya existe.");

        if (await _usuarioRepo.ExisteEmailAsync(dto.Email))
            return (false, "El email ya está registrado.");

        var usuario = new Usuario
        {
            NombreUsuario = dto.NombreUsuario,
            Nombre = dto.Nombre,
            Email = dto.Email,
            PasswordHash = HashPassword(dto.Password),
            RolId = dto.RolId,
            Activo = true,
            FechaCreacion = DateTime.UtcNow
        };

        await _usuarioRepo.CreateAsync(usuario);
        return (true, "Usuario registrado correctamente.");
    }

    // ─── LOGIN ────────────────────────────────────────────────────────────────
    public async Task<(bool Success, string Message, LoginResponseDto? Data)> LoginAsync(LoginRequestDto dto, string ipOrigen)
    {
        var usuario = await _usuarioRepo.GetByNombreUsuarioAsync(dto.NombreUsuario);

        if (usuario is null || !VerifyPassword(dto.Password, usuario.PasswordHash))
        {
            if (usuario is not null)
                await _usuarioRepo.RegistrarLogAsync(new LogAcceso
                {
                    UsuarioId = usuario.Id,
                    Fecha = DateTime.UtcNow,
                    Exitoso = false,
                    IpOrigen = ipOrigen,
                    Descripcion = "Credenciales incorrectas"
                });

            return (false, "Usuario o contraseña incorrectos.", null);
        }

        if (!usuario.Activo)
            return (false, "Usuario inactivo. Contacta al administrador.", null);

        // Verificar si tiene 2FA activo
        var config2FA = await _usuarioRepo.Get2FAAsync(usuario.Id);

        // No tiene 2FA configurado → debe configurarlo primero
        if (config2FA is null || !config2FA.Habilitado)
        {
            return (true, "Debes configurar el 2FA antes de continuar.", new LoginResponseDto
            {
                Requiere2FA = true,
                Configurado2FA = false,
                UsuarioId = usuario.Id,
                NombreUsuario = usuario.NombreUsuario
            });
        }

        // Ya tiene 2FA configurado → pedir código
        return (true, "Se requiere verificación 2FA.", new LoginResponseDto
        {
            Requiere2FA = true,
            Configurado2FA = true,
            UsuarioId = usuario.Id,
            NombreUsuario = usuario.NombreUsuario
        });
    }

    // ─── VERIFY 2FA ──────────────────────────────────────────────────────────
    public async Task<(bool Success, string Message, LoginResponseDto? Data)> Verify2FAAsync(Verify2FARequestDto dto, string ipOrigen)
    {
        var usuario = await _usuarioRepo.GetByIdAsync(dto.UsuarioId);
        if (usuario is null)
            return (false, "Usuario no encontrado.", null);

        var config2FA = await _usuarioRepo.Get2FAAsync(dto.UsuarioId);
        if (config2FA is null || !config2FA.Habilitado)
            return (false, "El usuario no tiene 2FA activo.", null);

        if (!ValidarCodigo2FA(config2FA.SecretKey, dto.Codigo))
        {
            await _usuarioRepo.RegistrarLogAsync(new LogAcceso
            {
                UsuarioId = usuario.Id,
                Fecha = DateTime.UtcNow,
                Exitoso = false,
                IpOrigen = ipOrigen,
                Descripcion = "Código 2FA incorrecto"
            });

            return (false, "Código 2FA incorrecto.", null);
        }

        var response = await GenerarTokensAsync(usuario, ipOrigen);
        return (true, "Login exitoso.", response);
    }

    // ─── REFRESH TOKEN ────────────────────────────────────────────────────────
    public async Task<(bool Success, string Message, LoginResponseDto? Data)> RefreshTokenAsync(RefreshTokenRequestDto dto)
    {
        var tokenSesion = await _usuarioRepo.GetTokenAsync(dto.RefreshToken);

        if (tokenSesion is null)
            return (false, "Refresh token inválido o revocado.", null);

        if (tokenSesion.FechaExpira < DateTime.UtcNow)
        {
            await _usuarioRepo.RevocarTokenAsync(dto.RefreshToken);
            return (false, "Refresh token expirado.", null);
        }

        var usuario = await _usuarioRepo.GetByIdAsync(tokenSesion.UsuarioId);
        if (usuario is null || !usuario.Activo)
            return (false, "Usuario no encontrado o inactivo.", null);

        await _usuarioRepo.RevocarTokenAsync(dto.RefreshToken);
        var response = await GenerarTokensAsync(usuario, tokenSesion.IpOrigen ?? "");
        return (true, "Token renovado.", response);
    }

    // ─── LOGOUT ───────────────────────────────────────────────────────────────
    public async Task<(bool Success, string Message)> LogoutAsync(RefreshTokenRequestDto dto)
    {
        await _usuarioRepo.RevocarTokenAsync(dto.RefreshToken);
        return (true, "Sesión cerrada correctamente.");
    }

    // ─── ENABLE 2FA ───────────────────────────────────────────────────────────
    public async Task<(bool Success, string Message, Enable2FAResponseDto? Data)> Enable2FAAsync(int usuarioId)
    {
        var usuario = await _usuarioRepo.GetByIdAsync(usuarioId);
        if (usuario is null)
            return (false, "Usuario no encontrado.", null);

        var secretKey = Base32Encoding.ToString(KeyGeneration.GenerateRandomKey(20));

        var config2FA = await _usuarioRepo.Get2FAAsync(usuarioId);
        if (config2FA is null)
        {
            await _usuarioRepo.Create2FAAsync(new Usuario2FA
            {
                UsuarioId = usuarioId,
                SecretKey = secretKey,
                Habilitado = false
            });
        }
        else
        {
            config2FA.SecretKey = secretKey;
            config2FA.Habilitado = false;
            await _usuarioRepo.Update2FAAsync(config2FA);
        }

        var qrUrl = $"otpauth://totp/BLL-Construccion:{usuario.NombreUsuario}?secret={secretKey}&issuer=BLL-Construccion";

        return (true, "Escanea el QR con Google Authenticator.", new Enable2FAResponseDto
        {
            SecretKey = secretKey,
            QrCodeUrl = qrUrl
        });
    }

    // ─── CONFIRM 2FA ──────────────────────────────────────────────────────────
    public async Task<(bool Success, string Message)> Confirm2FAAsync(Verify2FARequestDto dto)
    {
        var config2FA = await _usuarioRepo.Get2FAAsync(dto.UsuarioId);
        if (config2FA is null)
            return (false, "Primero activa el 2FA.");

        if (!ValidarCodigo2FA(config2FA.SecretKey, dto.Codigo))
            return (false, "Código incorrecto. Intenta de nuevo.");

        config2FA.Habilitado = true;
        config2FA.FechaActivado = DateTime.UtcNow;
        await _usuarioRepo.Update2FAAsync(config2FA);

        return (true, "2FA activado correctamente.");
    }

    // ─── HELPERS PRIVADOS ─────────────────────────────────────────────────────
    private async Task<LoginResponseDto> GenerarTokensAsync(Usuario usuario, string ipOrigen)
    {
        var accessToken = GenerarJWT(usuario);
        var refreshToken = GenerarRefreshToken();
        var expira = DateTime.UtcNow.AddMinutes(
            int.Parse(_config["Jwt:AccessTokenMinutes"]!));

        await _usuarioRepo.CreateTokenAsync(new TokenSesion
        {
            UsuarioId = usuario.Id,
            Token = refreshToken,
            FechaExpira = DateTime.UtcNow.AddDays(
                int.Parse(_config["Jwt:RefreshTokenDays"]!)),
            Revocado = false,
            FechaCreacion = DateTime.UtcNow,
            IpOrigen = ipOrigen
        });

        usuario.UltimoAcceso = DateTime.UtcNow;
        await _usuarioRepo.UpdateAsync(usuario);

        await _usuarioRepo.RegistrarLogAsync(new LogAcceso
        {
            UsuarioId = usuario.Id,
            Fecha = DateTime.UtcNow,
            Exitoso = true,
            IpOrigen = ipOrigen,
            Descripcion = "Login exitoso"
        });

        return new LoginResponseDto
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            Expira = expira,
            Requiere2FA = false,
            UsuarioId = usuario.Id,
            NombreUsuario = usuario.NombreUsuario
        };
    }

    private string GenerarJWT(Usuario usuario)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:SecretKey"]!));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var expira = DateTime.UtcNow.AddMinutes(int.Parse(_config["Jwt:AccessTokenMinutes"]!));

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub,   usuario.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, usuario.Email),
            new Claim("nombreUsuario",               usuario.NombreUsuario),
            new Claim("rolId",                       usuario.RolId.ToString()),
            new Claim(JwtRegisteredClaimNames.Jti,   Guid.NewGuid().ToString())
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

    private static string GenerarRefreshToken()
    {
        var bytes = RandomNumberGenerator.GetBytes(64);
        return Convert.ToBase64String(bytes);
    }

    private static string HashPassword(string password)
        => BCrypt.Net.BCrypt.HashPassword(password);

    private static bool VerifyPassword(string password, string hash)
        => BCrypt.Net.BCrypt.Verify(password, hash);

    private static bool ValidarCodigo2FA(string secretKey, string codigo)
    {
        var keyBytes = Base32Encoding.ToBytes(secretKey);
        var totp = new Totp(keyBytes);
        return totp.VerifyTotp(codigo, out _, VerificationWindow.RfcSpecifiedNetworkDelay);
    }
}