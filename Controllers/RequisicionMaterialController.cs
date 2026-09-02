using BLL_ConstruccionAPI.DTOs.Proyectos;
using BLL_ConstruccionAPI.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BLL_ConstruccionAPI.Controllers;

[Authorize]
[ApiController]
public class RequisicionMaterialController : ControllerBase
{
    private readonly IRequisicionMaterialService _service;

    public RequisicionMaterialController(IRequisicionMaterialService service)
    {
        _service = service;
    }

    // GET api/proyectos/{id}/requisiciones-material
    [HttpGet("api/proyectos/{id:int}/requisiciones-material")]
    public async Task<IActionResult> GetByProyecto(int id)
        => Ok(await _service.GetByProyectoAsync(id));

    // GET api/requisiciones-material/{id}
    [HttpGet("api/requisiciones-material/{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var data = await _service.GetByIdAsync(id);
        if (data is null) return NotFound(new { message = "Requisición no encontrada." });
        return Ok(data);
    }

    // POST api/proyectos/{id}/requisiciones-material
    [HttpPost("api/proyectos/{id:int}/requisiciones-material")]
    public async Task<IActionResult> Create(int id, [FromBody] RequisicionMaterialRequestDto dto)
    {
        var (success, message, data) = await _service.CreateAsync(id, dto);
        if (!success) return BadRequest(new { message });
        return Created(string.Empty, new { message, data });
    }

    // DELETE api/requisiciones-material/{id}
    [HttpDelete("api/requisiciones-material/{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var (success, message) = await _service.DeleteAsync(id);
        if (!success) return NotFound(new { message });
        return Ok(new { message });
    }

    // GET api/requisiciones-material/{id}/pdf
    [HttpGet("api/requisiciones-material/{id:int}/pdf")]
    public async Task<IActionResult> DescargarPdf(int id)
    {
        var (success, message, pdf) = await _service.GenerarPdfAsync(id);
        if (!success) return NotFound(new { message });
        return File(pdf!, "application/pdf", $"Requisicion_Materiales_{id}_{DateTime.UtcNow:yyyyMMdd}.pdf");
    }
}
