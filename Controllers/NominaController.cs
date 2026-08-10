using BLL_ConstruccionAPI.DTOs.Nomina;
using BLL_ConstruccionAPI.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BLL_ConstruccionAPI.Controllers;

[Authorize]
[ApiController]
[Route("api/nomina")]
public class NominaController : ControllerBase
{
    private readonly INominaService _service;

    public NominaController(INominaService service)
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

    // GET api/nomina/periodos
    [HttpGet("periodos")]
    public async Task<IActionResult> GetPeriodos()
    {
        if (!EsAdminOSistemas()) return Forbid();
        return Ok(await _service.GetPeriodosAsync());
    }

    // GET api/nomina/periodos/{id}
    [HttpGet("periodos/{id:int}")]
    public async Task<IActionResult> GetPeriodo(int id)
    {
        if (!EsAdminOSistemas()) return Forbid();
        var periodo = await _service.GetPeriodoByIdAsync(id);
        if (periodo is null) return NotFound(new { message = "Periodo de nómina no encontrado." });
        return Ok(periodo);
    }

    // POST api/nomina/periodos/generar
    [HttpPost("periodos/generar")]
    public async Task<IActionResult> GenerarPeriodo([FromBody] GenerarPeriodoNominaRequestDto dto)
    {
        if (!EsAdminOSistemas()) return Forbid();
        var (success, message, data) = await _service.GenerarPeriodoAsync(dto, GetUsuarioId());
        if (!success) return BadRequest(new { message });
        return Created(string.Empty, new { message, data });
    }

    // PUT api/nomina/periodos/{id}/marcar-pagado
    [HttpPut("periodos/{id:int}/marcar-pagado")]
    public async Task<IActionResult> MarcarPeriodoPagado(int id)
    {
        if (!EsAdminOSistemas()) return Forbid();
        var (success, message) = await _service.MarcarPeriodoPagadoAsync(id);
        if (!success) return BadRequest(new { message });
        return Ok(new { message });
    }

    // PUT api/nomina/detalle/{id}/marcar-pagado
    [HttpPut("detalle/{id:int}/marcar-pagado")]
    public async Task<IActionResult> MarcarDetallePagado(int id)
    {
        if (!EsAdminOSistemas()) return Forbid();
        var (success, message) = await _service.MarcarDetallePagadoAsync(id);
        if (!success) return BadRequest(new { message });
        return Ok(new { message });
    }

    // GET api/nomina/empleados/{empleadoId}/historial
    [HttpGet("empleados/{empleadoId:int}/historial")]
    public async Task<IActionResult> GetHistorialEmpleado(int empleadoId)
    {
        if (!EsAdminOSistemas()) return Forbid();
        return Ok(await _service.GetHistorialEmpleadoAsync(empleadoId));
    }

    // GET api/nomina/costos-por-proyecto
    [HttpGet("costos-por-proyecto")]
    public async Task<IActionResult> GetCostosPorProyecto()
    {
        if (!EsAdminOSistemas()) return Forbid();
        return Ok(await _service.GetCostosPorProyectoAsync());
    }

    // GET api/nomina/pagos
    [HttpGet("pagos")]
    public async Task<IActionResult> GetPagos()
    {
        if (!EsAdminOSistemas()) return Forbid();
        return Ok(await _service.GetPagosAsync());
    }

    // GET api/nomina/periodos/{id}/reporte/pdf
    [HttpGet("periodos/{id:int}/reporte/pdf")]
    public async Task<IActionResult> GetReportePdf(int id)
    {
        if (!EsAdminOSistemas()) return Forbid();
        var pdf = await _service.GenerarReportePdfAsync(id);
        if (pdf.Length == 0) return NotFound(new { message = "Periodo de nómina no encontrado." });
        return File(pdf, "application/pdf", $"Nomina_{id}_{DateTime.UtcNow:yyyyMMdd}.pdf");
    }
}
