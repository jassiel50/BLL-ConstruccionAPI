using BLL_ConstruccionAPI.DTOs.Devoluciones;
using BLL_ConstruccionAPI.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BLL_ConstruccionAPI.Controllers;

[Authorize]
[ApiController]
[Route("api/devoluciones-material")]
public class DevolucionesMaterialController : ControllerBase
{
    private readonly IDevolucionesMaterialService _service;

    public DevolucionesMaterialController(IDevolucionesMaterialService service)
    {
        _service = service;
    }

    // GET api/devoluciones-material
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var devoluciones = await _service.GetAllAsync();
        return Ok(devoluciones);
    }

    // GET api/devoluciones-material/{id}
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var devolucion = await _service.GetByIdAsync(id);
        if (devolucion is null) return NotFound(new { message = "Devolución no encontrada." });
        return Ok(devolucion);
    }

    // GET api/devoluciones-material/proyecto/{proyectoId}
    [HttpGet("proyecto/{proyectoId:int}")]
    public async Task<IActionResult> GetByProyecto(int proyectoId)
    {
        var devoluciones = await _service.GetByProyectoAsync(proyectoId);
        return Ok(devoluciones);
    }

    // POST api/devoluciones-material
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] DevolucionMaterialRequestDto dto)
    {
        if (!int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var usuarioId))
            return Unauthorized(new { message = "Token inválido." });

        var (success, message, data) = await _service.CreateAsync(usuarioId, dto);
        if (!success) return BadRequest(new { message });
        return Created(string.Empty, new { message, data });
    }
}
