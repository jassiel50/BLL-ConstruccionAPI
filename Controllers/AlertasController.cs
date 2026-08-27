using BLL_ConstruccionAPI.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BLL_ConstruccionAPI.Controllers;

[Authorize]
[ApiController]
public class AlertasController : ControllerBase
{
    private readonly IAlertasService _service;

    public AlertasController(IAlertasService service)
    {
        _service = service;
    }

    private bool EsAdminOSistemas()
    {
        var rolId = User.FindFirstValue("rolId");
        return rolId == "1" || rolId == "3";
    }

    // GET api/alertas/resumen
    [HttpGet("api/alertas/resumen")]
    public async Task<IActionResult> GetResumen()
    {
        var resumen = await _service.GetResumenAsync();
        return Ok(resumen);
    }

    // GET api/alertas/stock-bajo
    [HttpGet("api/alertas/stock-bajo")]
    public async Task<IActionResult> GetStockBajo()
    {
        var alertas = await _service.GetStockBajoAsync();
        return Ok(alertas);
    }

    // GET api/alertas/fases-atrasadas
    [HttpGet("api/alertas/fases-atrasadas")]
    public async Task<IActionResult> GetFasesAtrasadas()
    {
        var alertas = await _service.GetFasesAtrasadasAsync();
        return Ok(alertas);
    }

    // GET api/alertas/fases-por-vencer
    [HttpGet("api/alertas/fases-por-vencer")]
    public async Task<IActionResult> GetFasesPorVencer()
    {
        var alertas = await _service.GetFasesPorVencerAsync();
        return Ok(alertas);
    }

    // GET api/alertas/proyectos-sin-fases
    [HttpGet("api/alertas/proyectos-sin-fases")]
    public async Task<IActionResult> GetProyectosSinFases()
    {
        var alertas = await _service.GetProyectosSinFasesAsync();
        return Ok(alertas);
    }

    // GET api/alertas/herramientas-sin-devolver
    [HttpGet("api/alertas/herramientas-sin-devolver")]
    public async Task<IActionResult> GetHerramientasSinDevolver()
    {
        var alertas = await _service.GetHerramientasSinDevolverAsync();
        return Ok(alertas);
    }

    // GET api/alertas/sin-herramientas-disponibles
    [HttpGet("api/alertas/sin-herramientas-disponibles")]
    public async Task<IActionResult> GetSinHerramientasDisponibles()
    {
        var alertas = await _service.GetSinHerramientasDisponiblesAsync();
        return Ok(alertas);
    }

    // GET api/alertas/proyectos-con-fases-completadas
    [HttpGet("api/alertas/proyectos-con-fases-completadas")]
    public async Task<IActionResult> GetProyectosConFasesCompletadas()
    {
        var alertas = await _service.GetProyectosConFasesCompletadasAsync();
        return Ok(alertas);
    }

    // GET api/alertas/contratos-por-vencer
    [HttpGet("api/alertas/contratos-por-vencer")]
    public async Task<IActionResult> GetContratosPorVencer()
    {
        var alertas = await _service.GetContratosPorVencerAsync();
        return Ok(alertas);
    }

    // POST api/alertas/fases/reenviar/{usuarioId}
    // Envío manual (fuera del ciclo automático) de los correos de fases vencen
    // hoy/mañana/atrasadas a un usuario puntual. Útil cuando se agrega a alguien
    // como destinatario a media semana y no quiere esperar hasta el día siguiente.
    [HttpPost("api/alertas/fases/reenviar/{usuarioId:int}")]
    public async Task<IActionResult> ReenviarNotificacionesFases(int usuarioId)
    {
        if (!EsAdminOSistemas()) return Forbid();
        var (success, message) = await _service.ReenviarNotificacionesFasesAsync(usuarioId);
        if (!success) return BadRequest(new { message });
        return Ok(new { message });
    }
}
