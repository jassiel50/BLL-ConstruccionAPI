using BLL_ConstruccionAPI.DTOs.Cotizaciones;
using BLL_ConstruccionAPI.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BLL_ConstruccionAPI.Controllers;

[Authorize]
[ApiController]
[Route("api/cotizaciones")]
public class CotizacionesController : ControllerBase
{
    private readonly ICotizacionesService _service;

    public CotizacionesController(ICotizacionesService service)
    {
        _service = service;
    }

    // Usuarios con acceso temporal de prueba mientras el módulo se restringe por rol.
    // TODO: quitar esta excepción cuando se defina el rol/permiso definitivo para estos usuarios.
    private static readonly HashSet<int> UsuariosPruebaTemporal = [3]; // vannia.dionisio

    private bool EsAdminOSistemas()
    {
        var rolId = User.FindFirstValue("rolId");
        if (rolId == "1" || rolId == "3") return true;
        return UsuariosPruebaTemporal.Contains(GetUsuarioId());
    }

    private int GetUsuarioId()
    {
        int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var usuarioId);
        return usuarioId;
    }

    // GET api/cotizaciones
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        if (!EsAdminOSistemas()) return Forbid();
        return Ok(await _service.GetAllAsync());
    }

    // GET api/cotizaciones/{id}
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        if (!EsAdminOSistemas()) return Forbid();
        var data = await _service.GetByIdAsync(id);
        if (data is null) return NotFound(new { message = "Cotización no encontrada." });
        return Ok(data);
    }

    // POST api/cotizaciones/generar
    [HttpPost("generar")]
    public async Task<IActionResult> Generar([FromBody] CotizacionRequestDto dto)
    {
        if (!EsAdminOSistemas()) return Forbid();
        var (success, message, data) = await _service.GenerarAsync(dto, GetUsuarioId());
        if (!success) return BadRequest(new { message });
        return Created(string.Empty, new { message, data });
    }

    // PUT api/cotizaciones/{id}
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Actualizar(int id, [FromBody] CotizacionRequestDto dto)
    {
        if (!EsAdminOSistemas()) return Forbid();
        var (success, message, data) = await _service.ActualizarAsync(id, dto);
        if (!success) return BadRequest(new { message });
        return Ok(new { message, data });
    }

    // POST api/cotizaciones/borrador
    [HttpPost("borrador")]
    public async Task<IActionResult> GuardarBorradorNuevo([FromBody] CotizacionRequestDto dto)
    {
        if (!EsAdminOSistemas()) return Forbid();
        var (success, message, data) = await _service.GuardarBorradorNuevoAsync(dto, GetUsuarioId());
        if (!success) return BadRequest(new { message });
        return Created(string.Empty, new { message, data });
    }

    // PUT api/cotizaciones/{id}/borrador
    [HttpPut("{id:int}/borrador")]
    public async Task<IActionResult> GuardarBorradorExistente(int id, [FromBody] CotizacionRequestDto dto)
    {
        if (!EsAdminOSistemas()) return Forbid();
        var (success, message) = await _service.GuardarBorradorExistenteAsync(id, dto);
        if (!success) return BadRequest(new { message });
        return Ok(new { message });
    }

    // DELETE api/cotizaciones/{id}
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Eliminar(int id)
    {
        if (!EsAdminOSistemas()) return Forbid();
        var (success, message) = await _service.EliminarAsync(id);
        if (!success) return BadRequest(new { message });
        return Ok(new { message });
    }

    // GET api/cotizaciones/{id}/pdf
    [HttpGet("{id:int}/pdf")]
    public async Task<IActionResult> DescargarPdf(int id)
    {
        if (!EsAdminOSistemas()) return Forbid();
        var (found, contenido, folio) = await _service.DescargarAsync(id);
        if (!found || contenido is null) return NotFound(new { message = "Cotización no encontrada." });
        return File(contenido, "application/pdf", $"Cotizacion_{folio}.pdf");
    }
}
