using BLL_ConstruccionAPI.DTOs.Proyectos;
using BLL_ConstruccionAPI.Helpers;
using BLL_ConstruccionAPI.Repositories.Interfaces;
using BLL_ConstruccionAPI.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BLL_ConstruccionAPI.Controllers;

[Authorize]
[ApiController]
[Route("api/proyectos")]
public class ProyectoController : ControllerBase
{
    private readonly IProyectosService _service;
    private readonly IReportesService _reportesService;
    private readonly IEmailService _emailService;
    private readonly IUsuarioRepository _usuarioRepo;

    public ProyectoController(
        IProyectosService service,
        IReportesService reportesService,
        IEmailService emailService,
        IUsuarioRepository usuarioRepo)
    {
        _service = service;
        _reportesService = reportesService;
        _emailService = emailService;
        _usuarioRepo = usuarioRepo;
    }

    // GET api/proyectos
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var proyectos = await _service.GetAllAsync();
        return Ok(proyectos);
    }

    // GET api/proyectos/{id}
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var proyecto = await _service.GetByIdAsync(id);
        if (proyecto is null) return NotFound(new { message = "Proyecto no encontrado." });
        return Ok(proyecto);
    }

    // GET api/proyectos/cliente/{clienteId}
    [HttpGet("cliente/{clienteId:int}")]
    public async Task<IActionResult> GetByCliente(int clienteId)
    {
        var proyectos = await _service.GetByClienteAsync(clienteId);
        return Ok(proyectos);
    }

    // POST api/proyectos
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] ProyectoRequestDto dto)
    {
        if (!int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var userId))
            return Unauthorized(new { message = "Token inválido." });
        dto.ResponsableId = userId;

        var (success, message, data) = await _service.CreateAsync(dto);
        if (!success) return BadRequest(new { message });
        return CreatedAtAction(nameof(GetById), new { id = data!.Id }, new { message, data });
    }

    // PUT api/proyectos/{id}
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] ProyectoRequestDto dto)
    {
        if (!int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var userId))
            return Unauthorized(new { message = "Token inválido." });
        dto.ResponsableId = userId;

        var (success, message) = await _service.UpdateAsync(id, dto);
        if (!success) return BadRequest(new { message });
        return Ok(new { message });
    }

    // DELETE api/proyectos/{id}?liberarInventario=false
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, [FromQuery] bool liberarInventario = false)
    {
        var (success, message, inventario) = await _service.DeleteAsync(id, liberarInventario);
        if (!success)
        {
            if (inventario is not null)
                return Conflict(new { message, inventario });
            return NotFound(new { message });
        }
        return Ok(new { message });
    }

    // PUT api/proyectos/{id}/terminar
    [HttpPut("{id:int}/terminar")]
    public async Task<IActionResult> Terminar(int id)
    {
        var (success, message) = await _service.TerminarAsync(id);
        if (!success) return BadRequest(new { message });
        return Ok(new { message });
    }

    // GET api/proyectos/{id}/materiales
    [HttpGet("{id:int}/materiales")]
    public async Task<IActionResult> GetMateriales(int id)
    {
        var materiales = await _service.GetMaterialesAsync(id);
        return Ok(materiales);
    }

    // GET api/proyectos/{id}/herramientas
    [HttpGet("{id:int}/herramientas")]
    public async Task<IActionResult> GetHerramientas(int id)
    {
        var herramientas = await _service.GetHerramientasAsync(id);
        return Ok(herramientas);
    }

    // POST api/proyectos/{id}/devolver-herramientas
    [HttpPost("{id:int}/devolver-herramientas")]
    public async Task<IActionResult> DevolverHerramientas(int id)
    {
        var (success, message, count) = await _service.DevolverTodasHerramientasAsync(id);
        if (!success) return BadRequest(new { message });
        return Ok(new { count, message });
    }

    // GET api/proyectos/{id}/historial-financiero
    // Historial unificado y cronológico de ingresos (pagos) y gastos (materiales, herramientas, extras, semanales).
    [HttpGet("{id:int}/historial-financiero")]
    public async Task<IActionResult> GetHistorialFinanciero(int id)
    {
        var historial = await _service.GetHistorialFinancieroAsync(id);
        return Ok(historial);
    }

    // GET api/proyectos/{id}/planeacion/pdf
    [HttpGet("{id:int}/planeacion/pdf")]
    public async Task<IActionResult> DescargarPlaneacion(int id)
    {
        var (success, message, pdf) = await _service.GenerarPlaneacionAsync(id);
        if (!success) return BadRequest(new { message });
        return File(pdf!, "application/pdf", $"Planeacion_Proyecto_{id}_{DateTime.UtcNow:yyyyMMdd}.pdf");
    }

    // GET api/proyectos/{id}/avance/pdf
    // Reporte de avance + estado financiero completo (uso interno, no enviar a clientes).
    [HttpGet("{id:int}/avance/pdf")]
    public async Task<IActionResult> DescargarAvance(int id)
    {
        var pdf = await _reportesService.GenerarAvanceInternoAsync(id);
        if (pdf.Length == 0) return NotFound(new { message = "Proyecto no encontrado." });
        return File(pdf, "application/pdf", $"Avance_Proyecto_{id}_{DateTime.UtcNow:yyyyMMdd}.pdf");
    }

    // POST api/proyectos/{id}/avance/enviarme
    // Genera el mismo PDF de avance y lo manda al correo (o correos) del usuario autenticado.
    [HttpPost("{id:int}/avance/enviarme")]
    public async Task<IActionResult> EnviarmeAvance(int id)
    {
        if (!int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var userId))
            return Unauthorized(new { message = "Token inválido." });

        var usuario = await _usuarioRepo.GetByIdAsync(userId);
        if (usuario is null) return Unauthorized(new { message = "Usuario no encontrado." });

        var pdf = await _reportesService.GenerarAvanceInternoAsync(id);
        if (pdf.Length == 0) return NotFound(new { message = "Proyecto no encontrado." });

        var proyecto = await _service.GetByIdAsync(id);
        var nombreArchivo = $"Avance_Proyecto_{id}_{DateTime.UtcNow:yyyyMMdd}.pdf";

        foreach (var correo in usuario.CorreosNotificacion())
            await _emailService.SendReporteProgramadoAsync(correo, usuario.Nombre,
                $"Avance de Proyecto — {proyecto?.Nombre ?? $"#{id}"}", pdf, nombreArchivo);

        return Ok(new { message = "Reporte enviado a tu correo." });
    }

    // GET api/proyectos/{id}/avance-cliente/pdf
    // Versión SIN datos financieros internos (sin utilidad, presupuesto ni gasto real) —
    // pensada para compartirse manualmente con el cliente desde el Gantt.
    [HttpGet("{id:int}/avance-cliente/pdf")]
    public async Task<IActionResult> DescargarAvanceCliente(int id)
    {
        var pdf = await _reportesService.GenerarAvanceClienteAsync(id);
        if (pdf.Length == 0) return NotFound(new { message = "Proyecto no encontrado." });
        return File(pdf, "application/pdf", $"Avance_Proyecto_{id}_{DateTime.UtcNow:yyyyMMdd}.pdf");
    }

    // POST api/proyectos/{id}/avance-cliente/enviarme
    // Igual que el anterior pero enviado por correo al usuario autenticado (para que él
    // mismo lo reenvíe al cliente). Nunca incluye utilidad, presupuesto ni gasto real.
    [HttpPost("{id:int}/avance-cliente/enviarme")]
    public async Task<IActionResult> EnviarmeAvanceCliente(int id)
    {
        if (!int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var userId))
            return Unauthorized(new { message = "Token inválido." });

        var usuario = await _usuarioRepo.GetByIdAsync(userId);
        if (usuario is null) return Unauthorized(new { message = "Usuario no encontrado." });

        var pdf = await _reportesService.GenerarAvanceClienteAsync(id);
        if (pdf.Length == 0) return NotFound(new { message = "Proyecto no encontrado." });

        var proyecto = await _service.GetByIdAsync(id);
        var nombreArchivo = $"Avance_Proyecto_{id}_{DateTime.UtcNow:yyyyMMdd}.pdf";

        foreach (var correo in usuario.CorreosNotificacion())
            await _emailService.SendReporteProgramadoAsync(correo, usuario.Nombre,
                $"Avance de Proyecto (para cliente) — {proyecto?.Nombre ?? $"#{id}"}", pdf, nombreArchivo);

        return Ok(new { message = "Reporte enviado a tu correo." });
    }
}
