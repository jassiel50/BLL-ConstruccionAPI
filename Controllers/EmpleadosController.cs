using BLL_ConstruccionAPI.DTOs.Personal;
using BLL_ConstruccionAPI.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BLL_ConstruccionAPI.Controllers;

[Authorize]
[ApiController]
[Route("api/empleados")]
public class EmpleadosController : ControllerBase
{
    private readonly IEmpleadosService _service;

    public EmpleadosController(IEmpleadosService service)
    {
        _service = service;
    }

    private bool EsAdminOSistemas()
    {
        var rolId = User.FindFirstValue("rolId");
        return rolId == "1" || rolId == "3";
    }

    private int GetUsuarioId()
    {
        int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var usuarioId);
        return usuarioId;
    }

    // GET api/empleados
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        if (!EsAdminOSistemas()) return Forbid();
        return Ok(await _service.GetAllAsync());
    }

    // GET api/empleados/{id}
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        if (!EsAdminOSistemas()) return Forbid();
        var empleado = await _service.GetByIdAsync(id);
        if (empleado is null) return NotFound(new { message = "Empleado no encontrado." });
        return Ok(empleado);
    }

    // POST api/empleados
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] EmpleadoRequestDto dto)
    {
        if (!EsAdminOSistemas()) return Forbid();
        var (success, message, data) = await _service.CreateAsync(dto);
        if (!success) return BadRequest(new { message });
        return Created(string.Empty, new { message, data });
    }

    // PUT api/empleados/{id}
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] EmpleadoRequestDto dto)
    {
        if (!EsAdminOSistemas()) return Forbid();
        var (success, message, data) = await _service.UpdateAsync(id, dto);
        if (!success) return BadRequest(new { message });
        return Ok(new { message, data });
    }

    // PUT api/empleados/{id}/toggle-estatus
    [HttpPut("{id:int}/toggle-estatus")]
    public async Task<IActionResult> ToggleEstatus(int id)
    {
        if (!EsAdminOSistemas()) return Forbid();
        var (success, message) = await _service.ToggleEstatusAsync(id);
        if (!success) return BadRequest(new { message });
        return Ok(new { message });
    }

    // ─── ARCHIVOS ───────────────────────────────────────────────────────────

    // GET api/empleados/{id}/archivos
    [HttpGet("{id:int}/archivos")]
    public async Task<IActionResult> GetArchivos(int id)
    {
        if (!EsAdminOSistemas()) return Forbid();
        return Ok(await _service.GetArchivosAsync(id));
    }

    // POST api/empleados/{id}/archivos
    // Form: archivo (IFormFile), tipoDocumento (string)
    [HttpPost("{id:int}/archivos")]
    [RequestSizeLimit(20 * 1024 * 1024)]
    public async Task<IActionResult> SubirArchivo(int id, IFormFile archivo, [FromForm] string tipoDocumento)
    {
        if (!EsAdminOSistemas()) return Forbid();
        if (archivo is null || archivo.Length == 0)
            return BadRequest(new { message = "Se requiere un archivo." });

        var (success, message, data) = await _service.SubirArchivoAsync(id, archivo, tipoDocumento);
        if (!success) return BadRequest(new { message });
        return Created(string.Empty, new { message, data });
    }

    // GET api/empleados/archivos/{archivoId}/descargar
    [HttpGet("archivos/{archivoId:int}/descargar")]
    public async Task<IActionResult> DescargarArchivo(int archivoId)
    {
        if (!EsAdminOSistemas()) return Forbid();
        var (found, nombreOriginal, contentType, contenido) = await _service.DescargarArchivoAsync(archivoId);
        if (!found) return NotFound(new { message = "Archivo no encontrado." });
        return File(contenido!, contentType, nombreOriginal);
    }

    // DELETE api/empleados/archivos/{archivoId}
    [HttpDelete("archivos/{archivoId:int}")]
    public async Task<IActionResult> EliminarArchivo(int archivoId)
    {
        if (!EsAdminOSistemas()) return Forbid();
        var (success, message) = await _service.EliminarArchivoAsync(archivoId);
        if (!success) return NotFound(new { message });
        return Ok(new { message });
    }

    // ─── ASIGNACIONES A PROYECTO ────────────────────────────────────────────

    // GET api/empleados/{id}/asignaciones
    [HttpGet("{id:int}/asignaciones")]
    public async Task<IActionResult> GetAsignaciones(int id)
    {
        if (!EsAdminOSistemas()) return Forbid();
        return Ok(await _service.GetAsignacionesAsync(id));
    }

    // POST api/empleados/{id}/asignaciones
    [HttpPost("{id:int}/asignaciones")]
    public async Task<IActionResult> Asignar(int id, [FromBody] AsignacionEmpleadoRequestDto dto)
    {
        if (!EsAdminOSistemas()) return Forbid();
        var (success, message, data) = await _service.AsignarAsync(id, dto, GetUsuarioId());
        if (!success) return BadRequest(new { message });
        return Created(string.Empty, new { message, data });
    }

    // PUT api/empleados/asignaciones/{asignacionId}/finalizar
    [HttpPut("asignaciones/{asignacionId:int}/finalizar")]
    public async Task<IActionResult> FinalizarAsignacion(int asignacionId, [FromBody] AsignacionEmpleadoFinalizarDto dto)
    {
        if (!EsAdminOSistemas()) return Forbid();
        var (success, message) = await _service.FinalizarAsignacionAsync(asignacionId, dto);
        if (!success) return BadRequest(new { message });
        return Ok(new { message });
    }

    // ─── CONTRATO ───────────────────────────────────────────────────────────

    // POST api/empleados/{id}/contrato/pdf
    [HttpPost("{id:int}/contrato/pdf")]
    public async Task<IActionResult> GenerarContrato(int id, [FromBody] GenerarContratoRequestDto dto)
    {
        if (!EsAdminOSistemas()) return Forbid();
        var pdf = await _service.GenerarContratoAsync(id, dto);
        if (pdf.Length == 0) return NotFound(new { message = "Empleado no encontrado." });
        return File(pdf, "application/pdf", $"Contrato_{id}_{DateTime.UtcNow:yyyyMMdd}.pdf");
    }
}
