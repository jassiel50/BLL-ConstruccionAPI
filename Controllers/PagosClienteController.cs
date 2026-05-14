using BLL_ConstruccionAPI.DTOs.Pagos;
using BLL_ConstruccionAPI.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BLL_ConstruccionAPI.Controllers;

[Authorize]
[ApiController]
public class PagosClienteController : ControllerBase
{
    private readonly IPagosClienteService _service;

    public PagosClienteController(IPagosClienteService service)
    {
        _service = service;
    }

    // GET api/proyectos/{proyectoId}/pagos
    [HttpGet("api/proyectos/{proyectoId:int}/pagos")]
    public async Task<IActionResult> GetByProyecto(int proyectoId)
        => Ok(await _service.GetByProyectoAsync(proyectoId));

    // GET api/proyectos/{proyectoId}/pagos/resumen
    [HttpGet("api/proyectos/{proyectoId:int}/pagos/resumen")]
    public async Task<IActionResult> GetResumen(int proyectoId)
        => Ok(await _service.GetResumenAsync(proyectoId));

    // POST api/proyectos/{proyectoId}/pagos
    [HttpPost("api/proyectos/{proyectoId:int}/pagos")]
    public async Task<IActionResult> Create(int proyectoId, [FromBody] PagoClienteRequestDto dto)
    {
        var (success, message, data) = await _service.CreateAsync(proyectoId, dto);
        if (!success) return BadRequest(new { message });
        return Created(string.Empty, new { message, data });
    }

    // PUT api/pagos/{id}
    [HttpPut("api/pagos/{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] PagoClienteRequestDto dto)
    {
        var (success, message) = await _service.UpdateAsync(id, dto);
        if (!success) return NotFound(new { message });
        return Ok(new { message });
    }

    // DELETE api/pagos/{id}
    [HttpDelete("api/pagos/{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var (success, message) = await _service.DeleteAsync(id);
        if (!success) return NotFound(new { message });
        return NoContent();
    }

    // GET api/proyectos/{proyectoId}/pagos/pdf
    [HttpGet("api/proyectos/{proyectoId:int}/pagos/pdf")]
    public async Task<IActionResult> DescargarPdf(int proyectoId)
    {
        var (success, message, pdf) = await _service.GenerarPdfAsync(proyectoId);
        if (!success) return BadRequest(new { message });
        return File(pdf!, "application/pdf", $"Pagos_Proyecto_{proyectoId}_{DateTime.UtcNow:yyyyMMdd}.pdf");
    }
}
