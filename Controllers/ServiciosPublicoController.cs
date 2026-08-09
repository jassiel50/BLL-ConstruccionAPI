using BLL_ConstruccionAPI.DTOs.Servicios;
using BLL_ConstruccionAPI.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BLL_ConstruccionAPI.Controllers;

// Endpoints sin sesión para el flujo de "liga de un solo uso": un técnico de campo
// abre la liga que le compartieron (token en la URL), llena el servicio, sube evidencias
// y firma, todo sin necesidad de una cuenta. Deliberadamente separado de ServiciosController
// para no exponer por accidente rutas internas (listar todos los servicios, eliminar, reporte).
[AllowAnonymous]
[ApiController]
[Route("api/servicios-publico")]
public class ServiciosPublicoController : ControllerBase
{
    private readonly IServiciosService _service;

    public ServiciosPublicoController(IServiciosService service)
    {
        _service = service;
    }

    // GET api/servicios-publico/{token}
    [HttpGet("{token}")]
    public async Task<IActionResult> Get(string token)
    {
        var (valido, motivo, data) = await _service.GetPorTokenAsync(token);
        if (!valido) return NotFound(new { message = motivo });
        return Ok(new { message = "", data });
    }

    // PUT api/servicios-publico/{token}
    [HttpPut("{token}")]
    public async Task<IActionResult> Actualizar(string token, [FromBody] ServicioPublicoUpdateDto dto)
    {
        var (success, message, data) = await _service.ActualizarPorTokenAsync(token, dto);
        if (!success) return BadRequest(new { message });
        return Ok(new { message, data });
    }

    // POST api/servicios-publico/{token}/firmar
    [HttpPost("{token}/firmar")]
    public async Task<IActionResult> Firmar(string token, [FromBody] ServicioFirmarDto dto)
    {
        var (success, message, data) = await _service.FirmarPorTokenAsync(token, dto);
        if (!success) return BadRequest(new { message });
        return Ok(new { message, data });
    }

    // ─── EVIDENCIAS FOTOGRÁFICAS ────────────────────────────────────────────

    // GET api/servicios-publico/{token}/fotos
    [HttpGet("{token}/fotos")]
    public async Task<IActionResult> GetFotos(string token)
    {
        var (success, message, data) = await _service.GetFotosPorTokenAsync(token);
        if (!success) return BadRequest(new { message });
        return Ok(data);
    }

    // POST api/servicios-publico/{token}/fotos
    // Form: foto (IFormFile)
    [HttpPost("{token}/fotos")]
    [RequestSizeLimit(20 * 1024 * 1024)]
    public async Task<IActionResult> SubirFoto(string token, IFormFile foto)
    {
        if (foto is null || foto.Length == 0)
            return BadRequest(new { message = "Se requiere una foto." });

        var (success, message, data) = await _service.SubirFotoPorTokenAsync(token, foto);
        if (!success) return BadRequest(new { message });
        return Created(string.Empty, new { message, data });
    }

    // GET api/servicios-publico/{token}/fotos/{fotoId}/descargar
    [HttpGet("{token}/fotos/{fotoId:int}/descargar")]
    public async Task<IActionResult> DescargarFoto(string token, int fotoId)
    {
        var (found, nombreOriginal, contentType, contenido) = await _service.DescargarFotoPorTokenAsync(token, fotoId);
        if (!found) return NotFound(new { message = "Foto no encontrada." });
        return File(contenido!, contentType, nombreOriginal);
    }

    // DELETE api/servicios-publico/{token}/fotos/{fotoId}
    [HttpDelete("{token}/fotos/{fotoId:int}")]
    public async Task<IActionResult> EliminarFoto(string token, int fotoId)
    {
        var (success, message) = await _service.EliminarFotoPorTokenAsync(token, fotoId);
        if (!success) return BadRequest(new { message });
        return Ok(new { message });
    }
}
