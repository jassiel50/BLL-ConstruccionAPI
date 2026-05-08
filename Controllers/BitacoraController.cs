using BLL_ConstruccionAPI.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BLL_ConstruccionAPI.Controllers;

[Authorize]
[ApiController]
[Route("api/bitacora")]
public class BitacoraController : ControllerBase
{
    private readonly IBitacoraService _bitacoraService;

    public BitacoraController(IBitacoraService bitacoraService)
    {
        _bitacoraService = bitacoraService;
    }

    // GET api/bitacora
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var rolId = User.FindFirstValue("rolId");
        if (rolId != "1" && rolId != "3") return Forbid();
        var result = await _bitacoraService.GetAllAsync();
        return Ok(result);
    }

    // GET api/bitacora/usuario/{usuarioId}
    [HttpGet("usuario/{usuarioId:int}")]
    public async Task<IActionResult> GetByUsuario(int usuarioId)
    {
        var rolId = User.FindFirstValue("rolId");
        if (rolId != "1" && rolId != "3") return Forbid();
        var result = await _bitacoraService.GetByUsuarioAsync(usuarioId);
        return Ok(result);
    }
}
