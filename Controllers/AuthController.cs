using BLL_ConstruccionAPI.DTOs.Auth;
using BLL_ConstruccionAPI.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace BLL_ConstruccionAPI.Controllers;

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
        var ip = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "";
        var (success, message) = await _authService.RegisterAsync(dto, ip);

        if (!success) return BadRequest(new { message });
        return Ok(new { message });
    }

    // POST api/auth/login
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequestDto dto)
    {
        var ip = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "";
        var (success, message, data) = await _authService.LoginAsync(dto, ip);

        if (!success) return Unauthorized(new { message });
        return Ok(new { message, data });
    }

    // POST api/auth/login/2fa
    [HttpPost("login/2fa")]
    public async Task<IActionResult> LoginWith2FA([FromBody] Verify2FARequestDto dto)
    {
        var ip = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "";
        var (success, message, data) = await _authService.Verify2FAAsync(dto, ip);

        if (!success) return Unauthorized(new { message });
        return Ok(new { message, data });
    }

    // POST api/auth/refresh
    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh([FromBody] RefreshTokenRequestDto dto)
    {
        var (success, message, data) = await _authService.RefreshTokenAsync(dto);

        if (!success) return Unauthorized(new { message });
        return Ok(new { message, data });
    }

    // POST api/auth/logout
    [HttpPost("logout")]
    public async Task<IActionResult> Logout([FromBody] RefreshTokenRequestDto dto)
    {
        var (success, message) = await _authService.LogoutAsync(dto);

        if (!success) return BadRequest(new { message });
        return Ok(new { message });
    }

    // POST api/auth/2fa/enable
    [HttpPost("2fa/enable")]
    public async Task<IActionResult> Enable2FA([FromBody] int usuarioId)
    {
        var (success, message, data) = await _authService.Enable2FAAsync(usuarioId);

        if (!success) return BadRequest(new { message });
        return Ok(new { message, data });
    }

    // POST api/auth/2fa/verify
    [HttpPost("2fa/verify")]
    public async Task<IActionResult> Confirm2FA([FromBody] Verify2FARequestDto dto)
    {
        var (success, message) = await _authService.Confirm2FAAsync(dto);

        if (!success) return BadRequest(new { message });
        return Ok(new { message });
    }
}