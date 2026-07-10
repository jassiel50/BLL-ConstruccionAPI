using BLL_ConstruccionAPI.DTOs.Archivos;
using BLL_ConstruccionAPI.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BLL_ConstruccionAPI.Controllers;

[Authorize]
[ApiController]
public class ArchivosProyectoController : ControllerBase
{
    private readonly IArchivosProyectoService _service;

    public ArchivosProyectoController(IArchivosProyectoService service)
    {
        _service = service;
    }

    // GET api/proyectos/{proyectoId}/archivos
    [HttpGet("api/proyectos/{proyectoId:int}/archivos")]
    public async Task<IActionResult> GetByProyecto(int proyectoId)
        => Ok(await _service.GetByProyectoAsync(proyectoId));

    // POST api/proyectos/{proyectoId}/archivos
    // Form: archivo (IFormFile), tipoDocumento (string), carpetaId (int?, opcional)
    [HttpPost("api/proyectos/{proyectoId:int}/archivos")]
    [RequestSizeLimit(20 * 1024 * 1024)]
    public async Task<IActionResult> Subir(int proyectoId, IFormFile archivo, [FromForm] string tipoDocumento, [FromForm] int? carpetaId)
    {
        if (archivo is null || archivo.Length == 0)
            return BadRequest(new { message = "Se requiere un archivo." });

        var (success, message, data) = await _service.SubirAsync(proyectoId, archivo, tipoDocumento, carpetaId);
        if (!success) return BadRequest(new { message });
        return Created(string.Empty, new { message, data });
    }

    // GET api/archivos/{id}/descargar
    [HttpGet("api/archivos/{id:int}/descargar")]
    public async Task<IActionResult> Descargar(int id)
    {
        var (found, nombreOriginal, contentType, contenido) = await _service.DescargarAsync(id);
        if (!found) return NotFound(new { message = "Archivo no encontrado." });
        return File(contenido!, contentType, nombreOriginal);
    }

    // DELETE api/archivos/{id}
    [HttpDelete("api/archivos/{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var (success, message) = await _service.DeleteAsync(id);
        if (!success) return NotFound(new { message });
        return NoContent();
    }

    // GET api/proyectos/{proyectoId}/carpetas
    [HttpGet("api/proyectos/{proyectoId:int}/carpetas")]
    public async Task<IActionResult> GetCarpetas(int proyectoId)
        => Ok(await _service.GetCarpetasAsync(proyectoId));

    // POST api/proyectos/{proyectoId}/carpetas
    [HttpPost("api/proyectos/{proyectoId:int}/carpetas")]
    public async Task<IActionResult> CrearCarpeta(int proyectoId, [FromBody] CarpetaProyectoRequestDto dto)
    {
        var (success, message, data) = await _service.CrearCarpetaAsync(proyectoId, dto);
        if (!success) return BadRequest(new { message });
        return Created(string.Empty, new { message, data });
    }

    // DELETE api/carpetas/{id}
    [HttpDelete("api/carpetas/{id:int}")]
    public async Task<IActionResult> EliminarCarpeta(int id)
    {
        var (success, message) = await _service.EliminarCarpetaAsync(id);
        if (!success) return NotFound(new { message });
        return Ok(new { message });
    }
}
