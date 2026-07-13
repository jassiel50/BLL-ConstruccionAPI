using BLL_ConstruccionAPI.Data;
using BLL_ConstruccionAPI.Models.Enums;
using BLL_ConstruccionAPI.Reports;
using BLL_ConstruccionAPI.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Fluent;

namespace BLL_ConstruccionAPI.Services;

public class ReportesService : IReportesService
{
    private readonly AppDbContext _context;

    public ReportesService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<byte[]> GenerarInventarioAsync()
    {
        var stock = await _context.AlmacenCentral
            .AsNoTracking()
            .Include(ac => ac.Material)
                .ThenInclude(m => m!.Categoria)
            .Where(ac => ac.Material != null && ac.Material.Activo)
            .ToListAsync();

        return Document.Create(container =>
            new ReporteInventarioDocument(stock).Compose(container))
            .GeneratePdf();
    }

    public async Task<byte[]> GenerarMovimientosAsync(DateTime desde, DateTime hasta)
    {
        var entradas = await _context.Entradas
            .AsNoTracking()
            .Include(e => e.Proveedor)
            .Include(e => e.Detalles)
                .ThenInclude(d => d.Material)
            .Where(e => e.Fecha >= desde && e.Fecha <= hasta)
            .OrderByDescending(e => e.Fecha)
            .ToListAsync();

        var salidas = await _context.Salidas
            .AsNoTracking()
            .Include(s => s.Proyecto)
            .Include(s => s.Detalles)
                .ThenInclude(d => d.Material)
            .Where(s => s.Fecha >= desde && s.Fecha <= hasta)
            .OrderByDescending(s => s.Fecha)
            .ToListAsync();

        var devoluciones = await _context.DevolucionesMaterial
            .AsNoTracking()
            .Include(d => d.Material)
            .Include(d => d.Proyecto)
            .Where(d => d.FechaDevolucion >= desde && d.FechaDevolucion <= hasta)
            .OrderByDescending(d => d.FechaDevolucion)
            .ToListAsync();

        return Document.Create(container =>
            new ReporteMovimientosDocument(entradas, salidas, devoluciones, desde, hasta).Compose(container))
            .GeneratePdf();
    }

    public async Task<byte[]> GenerarHerramientasAsync()
    {
        var herramientas = await _context.Herramientas
            .AsNoTracking()
            .Include(h => h.CategoriaHerramienta)
            .Where(h => h.Activo)
            .OrderBy(h => h.Nombre)
            .ToListAsync();

        var asignaciones = await _context.AsignacionesHerramienta
            .AsNoTracking()
            .Include(a => a.Herramienta)
            .Include(a => a.Proyecto)
            .Where(a => a.Estado == EstadoAsignacion.Asignada)
            .OrderBy(a => a.FechaAsignacion)
            .ToListAsync();

        return Document.Create(container =>
            new ReporteHerramientasDocument(herramientas, asignaciones).Compose(container))
            .GeneratePdf();
    }

    public async Task<byte[]> GenerarProyectosAsync()
    {
        var proyectos = await _context.Proyectos
            .AsNoTracking()
            .Include(p => p.Cliente)
            .Where(p => p.Activo)
            .OrderBy(p => p.Nombre)
            .ToListAsync();

        var fases = await _context.FaseProyectos
            .AsNoTracking()
            .OrderBy(f => f.Orden)
            .ToListAsync();

        return Document.Create(container =>
            new ReporteProyectosDocument(proyectos, fases).Compose(container))
            .GeneratePdf();
    }

    public async Task<byte[]> GenerarPerdidasAsync(DateTime desde, DateTime hasta)
    {
        var perdidas = await _context.RegistrosPerdidas
            .AsNoTracking()
            .Include(r => r.Material)
            .Include(r => r.Herramienta)
            .Include(r => r.Proyecto)
            .Where(r => r.FechaPerdida >= desde && r.FechaPerdida <= hasta)
            .OrderByDescending(r => r.FechaPerdida)
            .ToListAsync();

        return Document.Create(container =>
            new ReportePerdidasDocument(perdidas, desde, hasta).Compose(container))
            .GeneratePdf();
    }

    public async Task<byte[]> GenerarPagosPorProyectoAsync(int proyectoId)
    {
        var proyecto = await _context.Proyectos.FindAsync(proyectoId);
        if (proyecto is null) return [];

        var pagos = await _context.PagosCliente
            .AsNoTracking()
            .Where(p => p.ProyectoId == proyectoId)
            .OrderBy(p => p.FechaPago)
            .ToListAsync();

        var totalPagado = pagos.Sum(p => p.Monto);
        var resumen = new DTOs.Pagos.ResumenPagosDto
        {
            ProyectoId = proyectoId,
            NombreProyecto = proyecto.Nombre,
            MontoContrato = proyecto.MontoContrato,
            TotalPagado = totalPagado,
            SaldoPendiente = proyecto.MontoContrato - totalPagado,
            NumeroPagos = pagos.Count,
            Pagos = pagos.Select(p => new DTOs.Pagos.PagoClienteDto
            {
                Id = p.Id, ProyectoId = p.ProyectoId, NombreProyecto = proyecto.Nombre,
                Concepto = p.Concepto, NumeroFactura = p.NumeroFactura, FechaCotizacion = p.FechaCotizacion,
                Subtotal = p.Subtotal, Iva = p.Iva, Total = p.Total, Monto = p.Monto, FechaPago = p.FechaPago,
                MetodoPago = p.MetodoPago, Referencia = p.Referencia, Estado = p.Estado.ToString(),
                ActividadStatus = p.ActividadStatus, Observaciones = p.Observaciones, FechaRegistro = p.FechaRegistro
            }).ToList()
        };

        return Document.Create(container =>
            new PagosDocument(resumen).Compose(container))
            .GeneratePdf();
    }

    public async Task<byte[]> GenerarAvanceClienteAsync(int proyectoId)
    {
        var proyecto = await _context.Proyectos
            .AsNoTracking()
            .Include(p => p.Cliente)
            .FirstOrDefaultAsync(p => p.Id == proyectoId);
        if (proyecto is null) return [];

        var fases = await _context.FaseProyectos
            .AsNoTracking()
            .Where(f => f.ProyectoId == proyectoId)
            .OrderBy(f => f.Orden)
            .ToListAsync();

        var pagos = await _context.PagosCliente
            .AsNoTracking()
            .Where(p => p.ProyectoId == proyectoId)
            .OrderBy(p => p.FechaPago)
            .ToListAsync();

        var totalPagado = pagos.Sum(p => p.Monto);
        var resumenPagos = new DTOs.Pagos.ResumenPagosDto
        {
            ProyectoId = proyectoId,
            NombreProyecto = proyecto.Nombre,
            MontoContrato = proyecto.MontoContrato,
            TotalPagado = totalPagado,
            SaldoPendiente = proyecto.MontoContrato - totalPagado,
            NumeroPagos = pagos.Count,
            Pagos = pagos.Select(p => new DTOs.Pagos.PagoClienteDto
            {
                Id = p.Id, ProyectoId = p.ProyectoId, NombreProyecto = proyecto.Nombre,
                Concepto = p.Concepto, NumeroFactura = p.NumeroFactura, FechaCotizacion = p.FechaCotizacion,
                Subtotal = p.Subtotal, Iva = p.Iva, Total = p.Total, Monto = p.Monto, FechaPago = p.FechaPago,
                MetodoPago = p.MetodoPago, Referencia = p.Referencia, Estado = p.Estado.ToString(),
                ActividadStatus = p.ActividadStatus, Observaciones = p.Observaciones, FechaRegistro = p.FechaRegistro
            }).ToList()
        };

        var fasesDto = fases.Select(DTOs.Fases.FaseResponseDto.FromEntity).ToList();

        return Document.Create(container =>
            new AvanceProyectoClienteDocument(proyecto, fasesDto, resumenPagos).Compose(container))
            .GeneratePdf();
    }
}
