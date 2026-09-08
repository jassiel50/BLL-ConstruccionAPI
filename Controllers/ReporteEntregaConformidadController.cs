using System.Security.Claims;
using BLL_ConstruccionAPI.DTOs.Reportes;
using BLL_ConstruccionAPI.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BLL_ConstruccionAPI.Controllers;

[Authorize]
[ApiController]
[Route("api/reportes-entrega")]
public class ReporteEntregaConformidadController : ControllerBase
{
    private readonly IReporteEntregaConformidadService _service;

    public ReporteEntregaConformidadController(IReporteEntregaConformidadService service)
    {
        _service = service;
    }

    private int GetUsuarioId()
    {
        int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var usuarioId);
        return usuarioId;
    }

    // GET api/reportes-entrega
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] int? proyectoId)
        => Ok(await _service.GetAllAsync(proyectoId));

    // GET api/reportes-entrega/{id}
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var data = await _service.GetByIdAsync(id);
        if (data is null) return NotFound(new { message = "Reporte no encontrado." });
        return Ok(data);
    }

    // POST api/reportes-entrega
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] ReporteEntregaConformidadRequestDto dto)
    {
        var (success, message, data) = await _service.CreateAsync(dto, GetUsuarioId());
        if (!success) return BadRequest(new { message });
        return Created(string.Empty, new { message, data });
    }

    // DELETE api/reportes-entrega/{id}
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var (success, message) = await _service.DeleteAsync(id);
        if (!success) return NotFound(new { message });
        return Ok(new { message });
    }

    // POST api/reportes-entrega/{id}/fotos
    [HttpPost("{id:int}/fotos")]
    [RequestSizeLimit(20 * 1024 * 1024)]
    public async Task<IActionResult> AgregarFotos(int id, [FromForm] List<IFormFile> fotos)
    {
        var (success, message) = await _service.AgregarFotosAsync(id, fotos);
        if (!success) return BadRequest(new { message });
        return Ok(new { message });
    }

    // GET api/reportes-entrega/fotos/{fotoId}
    [HttpGet("fotos/{fotoId:int}")]
    public async Task<IActionResult> DescargarFoto(int fotoId)
    {
        var (found, contentType, contenido) = await _service.DescargarFotoAsync(fotoId);
        if (!found) return NotFound(new { message = "Foto no encontrada." });
        return File(contenido!, contentType);
    }

    // DELETE api/reportes-entrega/fotos/{fotoId}
    [HttpDelete("fotos/{fotoId:int}")]
    public async Task<IActionResult> EliminarFoto(int fotoId)
    {
        var (success, message) = await _service.EliminarFotoAsync(fotoId);
        if (!success) return NotFound(new { message });
        return Ok(new { message });
    }

    // POST api/reportes-entrega/{id}/documento-firmado
    [HttpPost("{id:int}/documento-firmado")]
    [RequestSizeLimit(20 * 1024 * 1024)]
    public async Task<IActionResult> SubirDocumentoFirmado(int id, IFormFile archivo)
    {
        if (archivo is null || archivo.Length == 0)
            return BadRequest(new { message = "Se requiere un archivo." });

        var (success, message) = await _service.SubirDocumentoFirmadoAsync(id, archivo);
        if (!success) return BadRequest(new { message });
        return Ok(new { message });
    }

    // GET api/reportes-entrega/{id}/documento-firmado
    [HttpGet("{id:int}/documento-firmado")]
    public async Task<IActionResult> DescargarDocumentoFirmado(int id)
    {
        var (found, nombreOriginal, contentType, contenido) = await _service.DescargarDocumentoFirmadoAsync(id);
        if (!found) return NotFound(new { message = "Este reporte no tiene documento firmado." });
        return File(contenido!, contentType!, nombreOriginal);
    }

    // GET api/reportes-entrega/{id}/pdf
    [HttpGet("{id:int}/pdf")]
    public async Task<IActionResult> DescargarPdf(int id)
    {
        var (success, message, pdf) = await _service.GenerarPdfAsync(id);
        if (!success || pdf is null) return NotFound(new { message });
        return File(pdf, "application/pdf", $"ReporteEntrega_{id}.pdf");
    }
}
