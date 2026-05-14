using BLL_ConstruccionAPI.DTOs.GastosExtras;
using BLL_ConstruccionAPI.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BLL_ConstruccionAPI.Controllers;

[Authorize]
[ApiController]
public class GastosExtrasController : ControllerBase
{
    private readonly IGastoExtraService _service;

    public GastosExtrasController(IGastoExtraService service)
    {
        _service = service;
    }

    // GET api/fases/{faseId}/gastos-extras
    [HttpGet("api/fases/{faseId:int}/gastos-extras")]
    public async Task<IActionResult> GetByFase(int faseId)
    {
        var items = await _service.GetByFaseAsync(faseId);
        return Ok(items);
    }

    // POST api/fases/{faseId}/gastos-extras
    [HttpPost("api/fases/{faseId:int}/gastos-extras")]
    public async Task<IActionResult> Create(int faseId, [FromBody] GastoExtraRequestDto dto)
    {
        var (success, data) = await _service.CreateAsync(faseId, dto);
        if (!success)
            return NotFound(new { message = "Fase no encontrada." });

        return CreatedAtAction(nameof(GetByFase), new { faseId }, data);
    }

    // PUT api/gastos-extras/{id}
    [HttpPut("api/gastos-extras/{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] GastoExtraRequestDto dto)
    {
        var (success, data) = await _service.UpdateAsync(id, dto);
        if (!success)
            return NotFound(new { message = "Gasto extra no encontrado." });
        return Ok(data);
    }

    // DELETE api/gastos-extras/{id}
    [HttpDelete("api/gastos-extras/{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var deleted = await _service.DeleteAsync(id);
        if (!deleted)
            return NotFound(new { message = "Gasto extra no encontrado." });

        return NoContent();
    }
}
