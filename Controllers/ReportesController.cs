using BLL_ConstruccionAPI.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BLL_ConstruccionAPI.Controllers;

[Authorize]
[ApiController]
[Route("api/reportes")]
public class ReportesController : ControllerBase
{
    private readonly IReportesService _service;

    public ReportesController(IReportesService service)
    {
        _service = service;
    }

    // GET api/reportes/inventario
    [HttpGet("inventario")]
    public async Task<IActionResult> GetInventario()
    {
        var pdf = await _service.GenerarInventarioAsync();
        return File(pdf, "application/pdf", "reporte-inventario.pdf");
    }

    // GET api/reportes/movimientos?desde=2024-01-01&hasta=2024-12-31
    [HttpGet("movimientos")]
    public async Task<IActionResult> GetMovimientos(
        [FromQuery] DateTime? desde,
        [FromQuery] DateTime? hasta)
    {
        var fechaDesde = desde ?? DateTime.UtcNow.AddMonths(-1);
        var fechaHasta = hasta ?? DateTime.UtcNow;
        var pdf = await _service.GenerarMovimientosAsync(fechaDesde, fechaHasta);
        return File(pdf, "application/pdf", "reporte-movimientos.pdf");
    }

    // GET api/reportes/herramientas
    [HttpGet("herramientas")]
    public async Task<IActionResult> GetHerramientas()
    {
        var pdf = await _service.GenerarHerramientasAsync();
        return File(pdf, "application/pdf", "reporte-herramientas.pdf");
    }

    // GET api/reportes/proyectos
    [HttpGet("proyectos")]
    public async Task<IActionResult> GetProyectos()
    {
        var pdf = await _service.GenerarProyectosAsync();
        return File(pdf, "application/pdf", "reporte-proyectos.pdf");
    }

    // GET api/reportes/perdidas?desde=2024-01-01&hasta=2024-12-31
    [HttpGet("perdidas")]
    public async Task<IActionResult> GetPerdidas(
        [FromQuery] DateTime? desde,
        [FromQuery] DateTime? hasta)
    {
        var fechaDesde = desde ?? DateTime.UtcNow.AddMonths(-1);
        var fechaHasta = hasta ?? DateTime.UtcNow;
        var pdf = await _service.GenerarPerdidasAsync(fechaDesde, fechaHasta);
        return File(pdf, "application/pdf", "reporte-perdidas.pdf");
    }
}
