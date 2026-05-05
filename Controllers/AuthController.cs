using BLL_ConstruccionAPI.DTOs.Auth;
using BLL_ConstruccionAPI.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BLL_ConstruccionAPI.Controllers;

[Authorize]
[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    // POST api/auth/register
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequestDto dto)
    {
        var rolId = User.FindFirstValue("rolId");
        if (rolId != "1") return Forbid();

        var ip = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "";
        var (success, message) = await _authService.RegisterAsync(dto, ip);

        if (!success) return BadRequest(new { message });
        return Ok(new { message });
    }

    // POST api/auth/login
    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequestDto dto)
    {
        var ip = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "";
        var (success, message, data) = await _authService.LoginAsync(dto, ip);

        if (!success) return Unauthorized(new { message });
        return Ok(new { message, data });
    }

    // POST api/auth/login/2fa
    [AllowAnonymous]
    [HttpPost("login/2fa")]
    public async Task<IActionResult> LoginWith2FA([FromBody] Verify2FARequestDto dto)
    {
        var ip = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "";
        var (success, message, data) = await _authService.Verify2FAAsync(dto, ip);

        if (!success) return Unauthorized(new { message });
        return Ok(new { message, data });
    }

    // POST api/auth/refresh
    [AllowAnonymous]
    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh([FromBody] RefreshTokenRequestDto dto)
    {
        var (success, message, data) = await _authService.RefreshTokenAsync(dto);

        if (!success) return Unauthorized(new { message });
        return Ok(new { message, data });
    }

    // POST api/auth/logout
    [AllowAnonymous]
    [HttpPost("logout")]
    public async Task<IActionResult> Logout([FromBody] RefreshTokenRequestDto dto)
    {
        var (success, message) = await _authService.LogoutAsync(dto);

        if (!success) return BadRequest(new { message });
        return Ok(new { message });
    }

    // POST api/auth/2fa/enable  (requiere usuario autenticado — desde perfil)
    [HttpPost("2fa/enable")]
    public async Task<IActionResult> Enable2FA()
    {
        if (!int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var usuarioId))
            return Unauthorized(new { message = "Token inválido." });

        var (success, message, data) = await _authService.Enable2FAAsync(usuarioId);

        if (!success) return BadRequest(new { message });
        return Ok(new { message, data });
    }

    // POST api/auth/2fa/setup/{usuarioId}  (AllowAnonymous — primer setup obligatorio)
    [AllowAnonymous]
    [HttpPost("2fa/setup/{usuarioId:int}")]
    public async Task<IActionResult> Setup2FA(int usuarioId)
    {
        var (success, message, data) = await _authService.Enable2FAAsync(usuarioId);
        if (!success) return BadRequest(new { message });
        return Ok(new { message, data });
    }

    // POST api/auth/2fa/verify  (requiere Bearer — desde perfil autenticado)
    [HttpPost("2fa/verify")]
    public async Task<IActionResult> Confirm2FA([FromBody] Verify2FARequestDto dto)
    {
        if (!int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var usuarioId))
            return Unauthorized(new { message = "Token inválido." });

        dto.UsuarioId = usuarioId;
        var (success, message) = await _authService.Confirm2FAAsync(dto);

        if (!success) return BadRequest(new { message });
        return Ok(new { message });
    }

    // POST api/auth/2fa/verify/{usuarioId}  (AllowAnonymous — confirmación durante primer setup)
    [AllowAnonymous]
    [HttpPost("2fa/verify/{usuarioId:int}")]
    public async Task<IActionResult> Confirm2FASetup(int usuarioId, [FromBody] Verify2FARequestDto dto)
    {
        dto.UsuarioId = usuarioId;
        var (success, message) = await _authService.Confirm2FAAsync(dto);

        if (!success) return BadRequest(new { message });
        return Ok(new { message });
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // NUEVOS ENDPOINTS MFA FLEXIBLE
    // ═══════════════════════════════════════════════════════════════════════════

    // POST api/auth/mfa/setup/select-method
    /// <summary>Selecciona el método MFA inicial ("app" o "email") después de primer login.</summary>
    [AllowAnonymous]
    [HttpPost("mfa/setup/select-method")]
    public async Task<IActionResult> SelectMfaMethod([FromBody] SelectMfaMethodRequestDto dto)
    {
        var ip = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "";
        var (success, message, data) = await _authService.SelectMfaMethodAsync(dto, ip);

        if (!success) return BadRequest(new { message });
        return Ok(new { message, data });
    }

    // POST api/auth/mfa/setup/confirm-app
    /// <summary>Confirma el setup de MFA por app TOTP y activa el método.</summary>
    [AllowAnonymous]
    [HttpPost("mfa/setup/confirm-app")]
    public async Task<IActionResult> ConfirmMfaAppSetup([FromBody] ConfirmMfaAppSetupRequestDto dto)
    {
        var (success, message, data) = await _authService.ConfirmMfaAppSetupAsync(dto);

        if (!success) return BadRequest(new { message });
        return Ok(new { message, data });
    }

    // POST api/auth/mfa/email/send-code
    /// <summary>Envía un código OTP por email para verificación MFA en el flujo de login.</summary>
    [AllowAnonymous]
    [HttpPost("mfa/email/send-code")]
    public async Task<IActionResult> SendMfaEmailCode([FromBody] SendMfaEmailCodeRequestDto dto)
    {
        var ip = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "";
        var (success, message) = await _authService.SendMfaEmailCodeAsync(dto, ip);

        if (!success) return BadRequest(new { message });
        return Ok(new { message });
    }

    // POST api/auth/mfa/verify
    /// <summary>Verifica el código MFA (app o email) y devuelve los tokens de acceso.</summary>
    [AllowAnonymous]
    [HttpPost("mfa/verify")]
    public async Task<IActionResult> VerifyMfa([FromBody] VerifyMfaRequestDto dto)
    {
        var ip = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "";
        var (success, message, data) = await _authService.VerifyMfaAsync(dto, ip);

        if (!success) return Unauthorized(new { message });
        return Ok(new { message, data });
    }

    // POST api/auth/mfa/change-method
    /// <summary>Cambia el método MFA del usuario autenticado (requiere password + OTP actual).</summary>
    [HttpPost("mfa/change-method")]
    public async Task<IActionResult> ChangeMfaMethod([FromBody] ChangeMfaMethodRequestDto dto)
    {
        if (!int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var usuarioId))
            return Unauthorized(new { message = "Token inválido." });

        var ip = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "";
        var (success, message, data) = await _authService.ChangeMfaMethodAsync(dto, usuarioId, ip);

        if (!success) return BadRequest(new { message });
        return Ok(new { message, data });
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // PASSWORD RESET
    // ═══════════════════════════════════════════════════════════════════════════

    // POST api/auth/password-reset/request
    /// <summary>Solicita un código de recuperación de contraseña enviado al email.</summary>
    [AllowAnonymous]
    [HttpPost("password-reset/request")]
    public async Task<IActionResult> RequestPasswordReset([FromBody] RequestPasswordResetDto dto)
    {
        var ip = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "";
        var (success, message) = await _authService.RequestPasswordResetAsync(dto, ip);

        // Siempre OK para no filtrar si el email existe
        return Ok(new { message });
    }

    // POST api/auth/password-reset/verify
    /// <summary>Verifica que el código de recuperación sea válido (antes de mostrar formulario).</summary>
    [AllowAnonymous]
    [HttpPost("password-reset/verify")]
    public async Task<IActionResult> VerifyPasswordResetCode([FromBody] VerifyPasswordResetDto dto)
    {
        var ip = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "";
        var (success, message) = await _authService.VerifyPasswordResetCodeAsync(dto, ip);

        if (!success) return BadRequest(new { message });
        return Ok(new { message });
    }

    // POST api/auth/password-reset/confirm
    /// <summary>Establece la nueva contraseña usando el código de recuperación validado.</summary>
    [AllowAnonymous]
    [HttpPost("password-reset/confirm")]
    public async Task<IActionResult> ConfirmPasswordReset([FromBody] ConfirmPasswordResetDto dto)
    {
        var ip = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "";
        var (success, message) = await _authService.ConfirmPasswordResetAsync(dto, ip);

        if (!success) return BadRequest(new { message });
        return Ok(new { message });
    }
}