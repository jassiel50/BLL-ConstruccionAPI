using System.Security.Claims;
using BLL_ConstruccionAPI.Data;
using BLL_ConstruccionAPI.DTOs.Pagos;
using BLL_ConstruccionAPI.Models.Inventario.Proyectos;
using BLL_ConstruccionAPI.Reports;
using BLL_ConstruccionAPI.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Fluent;

namespace BLL_ConstruccionAPI.Services;

public class PagosClienteService : IPagosClienteService
{
    private readonly AppDbContext _context;
    private readonly IBitacoraService _bitacora;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public PagosClienteService(AppDbContext context, IBitacoraService bitacora, IHttpContextAccessor httpContextAccessor)
    {
        _context = context;
        _bitacora = bitacora;
        _httpContextAccessor = httpContextAccessor;
    }

    private (int Id, string Nombre, string Ip) GetUsuarioInfo()
    {
        var user = _httpContextAccessor.HttpContext?.User;
        var id = int.TryParse(user?.FindFirstValue(ClaimTypes.NameIdentifier), out var parsed) ? parsed : 0;
        var nombre = user?.FindFirstValue("nombreUsuario") ?? "Sistema";
        var ip = _httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString() ?? "";
        return (id, nombre, ip);
    }

    private static PagoClienteDto MapToDto(PagoCliente p, string nombreProyecto = "") => new()
    {
        Id = p.Id,
        ProyectoId = p.ProyectoId,
        NombreProyecto = p.Proyecto?.Nombre ?? nombreProyecto,
        Concepto = p.Concepto,
        Monto = p.Monto,
        FechaPago = p.FechaPago,
        MetodoPago = p.MetodoPago,
        Referencia = p.Referencia,
        Notas = p.Notas,
        FechaRegistro = p.FechaRegistro
    };

    public async Task<ResumenPagosDto> GetResumenAsync(int proyectoId)
    {
        var proyecto = await _context.Proyectos.FindAsync(proyectoId);
        if (proyecto is null)
            return new ResumenPagosDto { ProyectoId = proyectoId };

        var pagos = await _context.PagosCliente
            .Where(p => p.ProyectoId == proyectoId)
            .OrderBy(p => p.FechaPago)
            .ToListAsync();

        var totalPagado = pagos.Sum(p => p.Monto);
        return new ResumenPagosDto
        {
            ProyectoId = proyectoId,
            NombreProyecto = proyecto.Nombre,
            MontoContrato = proyecto.MontoContrato,
            TotalPagado = totalPagado,
            SaldoPendiente = proyecto.MontoContrato - totalPagado,
            NumeroPagos = pagos.Count,
            Pagos = pagos.Select(p => MapToDto(p, proyecto.Nombre)).ToList()
        };
    }

    public async Task<List<PagoClienteDto>> GetByProyectoAsync(int proyectoId) =>
        await _context.PagosCliente
            .Include(p => p.Proyecto)
            .Where(p => p.ProyectoId == proyectoId)
            .OrderBy(p => p.FechaPago)
            .Select(p => new PagoClienteDto
            {
                Id = p.Id, ProyectoId = p.ProyectoId, NombreProyecto = p.Proyecto!.Nombre,
                Concepto = p.Concepto, Monto = p.Monto, FechaPago = p.FechaPago,
                MetodoPago = p.MetodoPago, Referencia = p.Referencia,
                Notas = p.Notas, FechaRegistro = p.FechaRegistro
            })
            .ToListAsync();

    public async Task<(bool Success, string Message, PagoClienteDto? Data)> CreateAsync(int proyectoId, PagoClienteRequestDto dto)
    {
        var proyecto = await _context.Proyectos.FindAsync(proyectoId);
        if (proyecto is null || !proyecto.Activo) return (false, "Proyecto no encontrado.", null);

        var (uid, uname, ip) = GetUsuarioInfo();
        var entity = new PagoCliente
        {
            ProyectoId = proyectoId,
            Concepto = dto.Concepto,
            Monto = dto.Monto,
            FechaPago = dto.FechaPago,
            MetodoPago = dto.MetodoPago,
            Referencia = dto.Referencia,
            Notas = dto.Notas,
            RegistradoPorId = uid,
            FechaRegistro = DateTime.UtcNow
        };

        _context.PagosCliente.Add(entity);
        await _context.SaveChangesAsync();

        await _bitacora.RegistrarAsync(uid, uname, "Registró pago", "PagoCliente",
            $"Pago ${entity.Monto:N2} ({entity.Concepto}) registrado en proyecto ID {proyectoId}.", ip);

        entity.Proyecto = proyecto;
        return (true, "Pago registrado correctamente.", MapToDto(entity));
    }

    public async Task<(bool Success, string Message)> UpdateAsync(int id, PagoClienteRequestDto dto)
    {
        var entity = await _context.PagosCliente.FindAsync(id);
        if (entity is null) return (false, "Pago no encontrado.");

        entity.Concepto = dto.Concepto;
        entity.Monto = dto.Monto;
        entity.FechaPago = dto.FechaPago;
        entity.MetodoPago = dto.MetodoPago;
        entity.Referencia = dto.Referencia;
        entity.Notas = dto.Notas;

        await _context.SaveChangesAsync();

        var (uid, uname, ip) = GetUsuarioInfo();
        await _bitacora.RegistrarAsync(uid, uname, "Actualizó pago", "PagoCliente",
            $"Pago ID {entity.Id} actualizado.", ip);

        return (true, "Pago actualizado correctamente.");
    }

    public async Task<(bool Success, string Message)> DeleteAsync(int id)
    {
        var entity = await _context.PagosCliente.FindAsync(id);
        if (entity is null) return (false, "Pago no encontrado.");

        _context.PagosCliente.Remove(entity);
        await _context.SaveChangesAsync();

        var (uid, uname, ip) = GetUsuarioInfo();
        await _bitacora.RegistrarAsync(uid, uname, "Eliminó pago", "PagoCliente",
            $"Pago ID {entity.Id} eliminado.", ip);

        return (true, "Pago eliminado.");
    }

    public async Task<(bool Success, string Message, byte[]? Pdf)> GenerarPdfAsync(int proyectoId)
    {
        var resumen = await GetResumenAsync(proyectoId);
        if (resumen.NombreProyecto == string.Empty && resumen.ProyectoId == proyectoId)
            return (false, "Proyecto no encontrado.", null);

        var pdfBytes = Document.Create(container =>
            new PagosDocument(resumen).Compose(container)).GeneratePdf();

        return (true, "PDF generado.", pdfBytes);
    }
}
