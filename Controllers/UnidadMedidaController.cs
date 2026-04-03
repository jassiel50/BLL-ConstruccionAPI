using BLL_ConstruccionAPI.DTOs.Catalogos;
using BLL_ConstruccionAPI.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BLL_ConstruccionAPI.Controllers;

[Authorize]
[ApiController]
[Route("api/catalogos/unidades-medida")]
public class UnidadMedidaController : ControllerBase
{
    private readonly ICatalogosService _catalogosService;

    public UnidadMedidaController(ICatalogosService catalogosService)
    {
        _catalogosService = catalogosService;
    }

    // GET api/catalogos/unidades-medida
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var unidades = await _catalogosService.GetAllUnidadesAsync();
        return Ok(unidades);
    }

    // GET api/catalogos/unidades-medida/{id}
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var unidad = await _catalogosService.GetUnidadByIdAsync(id);
        if (unidad is null) return NotFound(new { message = "Unidad de medida no encontrada." });
        return Ok(unidad);
    }

    // POST api/catalogos/unidades-medida
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] UnidadMedidaRequestDto dto)
    {
        var (success, message, data) = await _catalogosService.CreateUnidadAsync(dto);
        if (!success) return BadRequest(new { message });
        return CreatedAtAction(nameof(GetById), new { id = data!.Id }, new { message, data });
    }

    // PUT api/catalogos/unidades-medida/{id}
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] UnidadMedidaRequestDto dto)
    {
        var (success, message) = await _catalogosService.UpdateUnidadAsync(id, dto);
        if (!success) return BadRequest(new { message });
        return Ok(new { message });
    }

    // DELETE api/catalogos/unidades-medida/{id}
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var (success, message) = await _catalogosService.DeleteUnidadAsync(id);
        if (!success) return NotFound(new { message });
        return Ok(new { message });
    }
}
