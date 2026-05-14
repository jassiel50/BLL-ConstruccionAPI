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
    // Form: archivo (IFormFile), tipoDocumento (string)
    [HttpPost("api/proyectos/{proyectoId:int}/archivos")]
    [RequestSizeLimit(20 * 1024 * 1024)]
    public async Task<IActionResult> Subir(int proyectoId, IFormFile archivo, [FromForm] string tipoDocumento)
    {
        if (archivo is null || archivo.Length == 0)
            return BadRequest(new { message = "Se requiere un archivo." });

        var (success, message, data) = await _service.SubirAsync(proyectoId, archivo, tipoDocumento);
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
}
