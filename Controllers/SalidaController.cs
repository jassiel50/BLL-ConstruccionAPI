using BLL_ConstruccionAPI.DTOs.Salidas;
using BLL_ConstruccionAPI.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace BLL_ConstruccionAPI.Controllers;

[ApiController]
[Route("api/salidas")]
public class SalidaController : ControllerBase
{
    private readonly ISalidasService _service;

    public SalidaController(ISalidasService service)
    {
        _service = service;
    }

    // GET api/salidas
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var salidas = await _service.GetAllAsync();
        return Ok(salidas);
    }

    // GET api/salidas/{id}
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var salida = await _service.GetByIdAsync(id);
        if (salida is null) return NotFound(new { message = "Salida no encontrada." });
        return Ok(salida);
    }

    // GET api/salidas/proyecto/{proyectoId}
    [HttpGet("proyecto/{proyectoId:int}")]
    public async Task<IActionResult> GetByProyecto(int proyectoId)
    {
        var salidas = await _service.GetByProyectoAsync(proyectoId);
        return Ok(salidas);
    }

    // GET api/salidas/almacen-proyecto/{proyectoId}
    [HttpGet("almacen-proyecto/{proyectoId:int}")]
    public async Task<IActionResult> GetAlmacenProyecto(int proyectoId)
    {
        var almacen = await _service.GetAlmacenProyectoAsync(proyectoId);
        return Ok(almacen);
    }

    // POST api/salidas
    [HttpPost]
    public async Task<IActionResult> Registrar([FromBody] SalidaRequestDto dto)
    {
        var (success, message, data) = await _service.RegistrarAsync(dto);
        if (!success) return BadRequest(new { message });
        return CreatedAtAction(nameof(GetById), new { id = data!.Id }, new { message, data });
    }
}
