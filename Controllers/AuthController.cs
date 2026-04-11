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
    [AllowAnonymous]
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequestDto dto)
    {
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
}