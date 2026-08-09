using BLL_ConstruccionAPI.DTOs.GastosSemanales;
using BLL_ConstruccionAPI.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BLL_ConstruccionAPI.Controllers;

[Authorize]
[ApiController]
public class GastosSemanalesController : ControllerBase
{
    private readonly IGastoSemanalService _service;

    public GastosSemanalesController(IGastoSemanalService service)
    {
        _service = service;
    }

    // GET api/proyectos/{id}/gastos-semanales
    [HttpGet("api/proyectos/{id:int}/gastos-semanales")]
    public async Task<IActionResult> GetByProyecto(int id)
        => Ok(await _service.GetByProyectoAsync(id));

    // GET api/proyectos/{id}/gastos-semanales/ultimo
    [HttpGet("api/proyectos/{id:int}/gastos-semanales/ultimo")]
    public async Task<IActionResult> GetUltimo(int id)
    {
        var (found, data) = await _service.GetUltimoAsync(id);
        if (!found) return NotFound(new { message = "No hay gastos semanales registrados para este proyecto." });
        return Ok(data);
    }

    // POST api/proyectos/{id}/gastos-semanales
    [HttpPost("api/proyectos/{id:int}/gastos-semanales")]
    public async Task<IActionResult> Create(int id, [FromBody] GastoSemanalRequestDto dto)
    {
        var (success, data) = await _service.CreateAsync(id, dto);
        if (!success) return NotFound(new { message = "Proyecto no encontrado." });
        return Created(string.Empty, data);
    }

    // DELETE api/gastos-semanales/{id}
    [HttpDelete("api/gastos-semanales/{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var success = await _service.DeleteAsync(id);
        if (!success) return NotFound(new { message = "Gasto semanal no encontrado." });
        return NoContent();
    }

    // GET api/proyectos/{id}/gastos-semanales/nomina-disponible
    [HttpGet("api/proyectos/{id:int}/gastos-semanales/nomina-disponible")]
    public async Task<IActionResult> GetNominaDisponible(int id)
        => Ok(await _service.GetNominaDisponibleParaProyectoAsync(id));

    // POST api/proyectos/{id}/gastos-semanales/desde-nomina
    [HttpPost("api/proyectos/{id:int}/gastos-semanales/desde-nomina")]
    public async Task<IActionResult> CrearDesdeNomina(int id, [FromBody] CrearGastoDesdeNominaDto dto)
    {
        var (success, message, data) = await _service.CrearDesdeNominaAsync(id, dto.PeriodoNominaId);
        if (!success) return BadRequest(new { message });
        return Created(string.Empty, data);
    }
}
