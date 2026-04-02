using BLL_ConstruccionAPI.DTOs.Entradas;
using BLL_ConstruccionAPI.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace BLL_ConstruccionAPI.Controllers;

[ApiController]
[Route("api/entradas")]
public class EntradaController : ControllerBase
{
    private readonly IEntradasService _service;

    public EntradaController(IEntradasService service)
    {
        _service = service;
    }

    // GET api/entradas
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var entradas = await _service.GetAllAsync();
        return Ok(entradas);
    }

    // GET api/entradas/{id}
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var entrada = await _service.GetByIdAsync(id);
        if (entrada is null) return NotFound(new { message = "Entrada no encontrada." });
        return Ok(entrada);
    }

    // POST api/entradas
    [HttpPost]
    public async Task<IActionResult> Registrar([FromBody] EntradaRequestDto dto)
    {
        var (success, message, data) = await _service.RegistrarAsync(dto);
        if (!success) return BadRequest(new { message });
        return CreatedAtAction(nameof(GetById), new { id = data!.Id }, new { message, data });
    }
}
