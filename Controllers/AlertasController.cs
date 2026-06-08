using BLL_ConstruccionAPI.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

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
}
