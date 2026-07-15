using BLL_ConstruccionAPI.DTOs.Herramientas;
using BLL_ConstruccionAPI.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BLL_ConstruccionAPI.Controllers;

[Authorize]
[ApiController]
[Route("api/herramientas")]
public class HerramientaController : ControllerBase
{
    private readonly IHerramientasService _service;

    public HerramientaController(IHerramientasService service)
    {
        _service = service;
    }

    // GET api/herramientas
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var herramientas = await _service.GetAllAsync();
        return Ok(herramientas);
    }

    // GET api/herramientas/disponibles
    [HttpGet("disponibles")]
    public async Task<IActionResult> GetDisponibles()
    {
        var herramientas = await _service.GetDisponiblesAsync();
        return Ok(herramientas);
    }

    // GET api/herramientas/{id}
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var herramienta = await _service.GetByIdAsync(id);
        if (herramienta is null) return NotFound(new { message = "Herramienta no encontrada." });
        return Ok(herramienta);
    }

    // GET api/herramientas/{id}/asignaciones
    [HttpGet("{id:int}/asignaciones")]
    public async Task<IActionResult> GetAsignaciones(int id)
    {
        var asignaciones = await _service.GetAsignacionesAsync(id);
        return Ok(asignaciones);
    }

    // POST api/herramientas
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] HerramientaRequestDto dto)
    {
        var (success, message, data) = await _service.CreateAsync(dto);
        if (!success) return BadRequest(new { message });
        return CreatedAtAction(nameof(GetById), new { id = data!.Id }, new { message, data });
    }

    // POST api/herramientas/bulk
    // Carga masiva: procesa cada herramienta con la misma validación que el alta individual
    // y devuelve un resultado por cada una (éxito o motivo de error), identificado por Codigo.
    [HttpPost("bulk")]
    public async Task<IActionResult> CreateBulk([FromBody] List<HerramientaRequestDto> dtos)
    {
        var resultados = await _service.CreateBulkAsync(dtos);
        return Ok(new { message = "Proceso de carga masiva finalizado.", data = resultados });
    }

    // PUT api/herramientas/{id}
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] HerramientaRequestDto dto)
    {
        var (success, message) = await _service.UpdateAsync(id, dto);
        if (!success) return BadRequest(new { message });
        return Ok(new { message });
    }

    // DELETE api/herramientas/{id}
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var (success, message) = await _service.DeleteAsync(id);
        if (!success) return BadRequest(new { message });
        return Ok(new { message });
    }

    // POST api/herramientas/asignar
    [HttpPost("asignar")]
    public async Task<IActionResult> Asignar([FromBody] AsignacionRequestDto dto)
    {
        if (!int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var usuarioId))
            return Unauthorized(new { message = "Token inválido." });

        var (success, message, data) = await _service.AsignarAsync(dto, usuarioId);
        if (!success) return BadRequest(new { message });
        return Ok(new { message, data });
    }

    // PUT api/herramientas/devolver/{asignacionId}
    [HttpPut("devolver/{asignacionId:int}")]
    public async Task<IActionResult> Devolver(int asignacionId, [FromBody] DevolucionRequestDto dto)
    {
        var (success, message) = await _service.DevolverAsync(asignacionId, dto);
        if (!success) return BadRequest(new { message });
        return Ok(new { message });
    }

    // GET api/herramientas/asignaciones
    // Todas las asignaciones activas del sistema (todas las herramientas, todos los proyectos)
    [HttpGet("asignaciones")]
    public async Task<IActionResult> GetAsignacionesActivas()
    {
        var asignaciones = await _service.GetAsignacionesActivasAsync();
        return Ok(asignaciones);
    }

    // POST api/herramientas/devolver-multiple
    [HttpPost("devolver-multiple")]
    public async Task<IActionResult> DevolverMultiple([FromBody] DevolverMultipleRequestDto dto)
    {
        var resultados = await _service.DevolverMultipleAsync(dto.AsignacionIds, dto.ObservacionesDevolucion);
        return Ok(new { message = "Proceso de devolución finalizado.", data = resultados });
    }

    // POST api/herramientas/transferir
    // Mueve una asignación activa de su proyecto actual a otro proyecto.
    [HttpPost("transferir")]
    public async Task<IActionResult> Transferir([FromBody] TransferirRequestDto dto)
    {
        if (!int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var usuarioId))
            return Unauthorized(new { message = "Token inválido." });

        var (success, message) = await _service.TransferirAsync(dto.AsignacionId, dto.NuevoProyectoId, usuarioId);
        if (!success) return BadRequest(new { message });
        return Ok(new { message });
    }

    // POST api/herramientas/transferir-multiple
    [HttpPost("transferir-multiple")]
    public async Task<IActionResult> TransferirMultiple([FromBody] TransferirMultipleRequestDto dto)
    {
        if (!int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var usuarioId))
            return Unauthorized(new { message = "Token inválido." });

        var resultados = await _service.TransferirMultipleAsync(dto.AsignacionIds, dto.NuevoProyectoId, usuarioId);
        return Ok(new { message = "Proceso de transferencia finalizado.", data = resultados });
    }

    // PUT api/herramientas/{id}/ubicacion
    // Cambia el TipoUbicacion (Almacen/Oficina) de una herramienta, incluso si está asignada.
    [HttpPut("{id:int}/ubicacion")]
    public async Task<IActionResult> CambiarUbicacion(int id, [FromBody] CambiarUbicacionRequestDto dto)
    {
        var (success, message) = await _service.CambiarUbicacionAsync(id, dto.TipoUbicacion);
        if (!success) return BadRequest(new { message });
        return Ok(new { message });
    }
}
