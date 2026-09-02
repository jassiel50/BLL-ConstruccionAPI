using BLL_ConstruccionAPI.Data;
using BLL_ConstruccionAPI.DTOs.Cotizaciones;
using BLL_ConstruccionAPI.Models.Cotizaciones;
using BLL_ConstruccionAPI.Reports;
using BLL_ConstruccionAPI.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Fluent;

namespace BLL_ConstruccionAPI.Services;

public class CotizacionesService : ICotizacionesService
{
    private const decimal TasaIva = 0.16m;

    private readonly AppDbContext _context;

    public CotizacionesService(AppDbContext context)
    {
        _context = context;
    }

    private static string ObtenerEmpresa(Cotizacion c) =>
        c.Cliente?.Nombre ?? (string.IsNullOrWhiteSpace(c.EmpresaNombreLibre) ? "-" : c.EmpresaNombreLibre!);

    private async Task<string> GenerarFolioAsync()
    {
        var hoy = DateTime.UtcNow.Date;
        var consecutivo = await _context.Cotizaciones.CountAsync(c => c.FechaCreacion.Date == hoy) + 1;
        return $"BLL{hoy:ddMMyy}{consecutivo}";
    }

    public async Task<List<CotizacionResponseDto>> GetAllAsync() =>
        await _context.Cotizaciones
            .AsNoTracking()
            .Include(c => c.Cliente)
            .OrderByDescending(c => c.FechaCreacion)
            .Select(c => new CotizacionResponseDto
            {
                Id = c.Id,
                Folio = c.Folio,
                Estado = c.Estado,
                Empresa = c.Cliente != null ? c.Cliente.Nombre : (c.EmpresaNombreLibre ?? "-"),
                ContactoNombre = c.ContactoNombre,
                Titulo = c.Titulo,
                FechaCotizacion = c.FechaCotizacion,
                Total = c.Total
            })
            .ToListAsync();

    public async Task<(bool Success, string Message, CotizacionResponseDto? Data)> GenerarAsync(CotizacionRequestDto dto, int usuarioId)
    {
        if (string.IsNullOrWhiteSpace(dto.Titulo))
            return (false, "Captura el título de la cotización.", null);
        if (dto.ClienteId is null && string.IsNullOrWhiteSpace(dto.EmpresaNombreLibre))
            return (false, "Selecciona un cliente del catálogo o captura el nombre de la empresa.", null);
        if (dto.Items.Count == 0)
            return (false, "Agrega al menos una partida a la cotización.", null);

        var folio = await GenerarFolioAsync();
        var subtotal = dto.Items.Sum(i => i.Total);
        var iva = Math.Round(subtotal * TasaIva, 2);
        var total = subtotal + iva;

        var entity = new Cotizacion
        {
            Folio = folio,
            Estado = "Generada",
            ClienteId = dto.ClienteId,
            EmpresaNombreLibre = dto.EmpresaNombreLibre,
            ContactoNombre = dto.ContactoNombre,
            Titulo = dto.Titulo,
            Introduccion = dto.Introduccion,
            AlcanceGeneral = dto.AlcanceGeneral,
            FechaCotizacion = DateTime.UtcNow.Date,
            TiempoEntregaDias = dto.TiempoEntregaDias,
            Clausulas = dto.Clausulas,
            ValidezDias = dto.ValidezDias,
            CondicionesPago = dto.CondicionesPago,
            MetodoPago = dto.MetodoPago,
            Subtotal = subtotal,
            Iva = iva,
            Total = total,
            CreadoPorId = usuarioId,
            Items = dto.Items.Select((i, idx) => new CotizacionItem
            {
                Orden = idx + 1,
                Descripcion = i.Descripcion,
                Cantidad = i.Cantidad,
                Unidad = i.Unidad,
                Total = i.Total
            }).ToList()
        };

        if (dto.ClienteId.HasValue)
            entity.Cliente = await _context.Clientes.FindAsync(dto.ClienteId.Value);

        var empresaTexto = ObtenerEmpresa(entity);
        entity.PdfContenido = Document.Create(container =>
            new CotizacionDocument(entity, empresaTexto).Compose(container))
            .GeneratePdf();

        _context.Cotizaciones.Add(entity);
        await _context.SaveChangesAsync();

        return (true, "Cotización generada correctamente.", new CotizacionResponseDto
        {
            Id = entity.Id,
            Folio = entity.Folio,
            Estado = entity.Estado,
            Empresa = empresaTexto,
            ContactoNombre = entity.ContactoNombre,
            Titulo = entity.Titulo,
            FechaCotizacion = entity.FechaCotizacion,
            Total = entity.Total
        });
    }

    public async Task<CotizacionDetalleDto?> GetByIdAsync(int id)
    {
        var c = await _context.Cotizaciones
            .AsNoTracking()
            .Include(c => c.Items)
            .FirstOrDefaultAsync(c => c.Id == id);
        if (c is null) return null;

        return new CotizacionDetalleDto
        {
            Id = c.Id,
            Folio = c.Folio,
            Estado = c.Estado,
            ClienteId = c.ClienteId,
            EmpresaNombreLibre = c.EmpresaNombreLibre,
            ContactoNombre = c.ContactoNombre,
            Titulo = c.Titulo,
            Introduccion = c.Introduccion,
            AlcanceGeneral = c.AlcanceGeneral,
            TiempoEntregaDias = c.TiempoEntregaDias,
            Clausulas = c.Clausulas,
            ValidezDias = c.ValidezDias,
            CondicionesPago = c.CondicionesPago,
            MetodoPago = c.MetodoPago,
            Items = c.Items.OrderBy(i => i.Orden).Select(i => new CotizacionItemDto
            {
                Descripcion = i.Descripcion,
                Cantidad = i.Cantidad,
                Unidad = i.Unidad,
                Total = i.Total
            }).ToList()
        };
    }

    public async Task<(bool Success, string Message, CotizacionResponseDto? Data)> ActualizarAsync(int id, CotizacionRequestDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Titulo))
            return (false, "Captura el título de la cotización.", null);
        if (dto.ClienteId is null && string.IsNullOrWhiteSpace(dto.EmpresaNombreLibre))
            return (false, "Selecciona un cliente del catálogo o captura el nombre de la empresa.", null);
        if (dto.Items.Count == 0)
            return (false, "Agrega al menos una partida a la cotización.", null);

        var entity = await _context.Cotizaciones
            .Include(c => c.Items)
            .FirstOrDefaultAsync(c => c.Id == id);
        if (entity is null)
            return (false, "Cotización no encontrada.", null);

        // Si era un borrador autoguardado, esta es la acción que lo finaliza: se le asigna
        // folio definitivo y pasa a "Generada" (igual que si se hubiera creado desde cero).
        var eraBorrador = entity.Estado == "Borrador";
        if (eraBorrador)
        {
            entity.Folio = await GenerarFolioAsync();
            entity.Estado = "Generada";
            entity.FechaCotizacion = DateTime.UtcNow.Date;
        }

        var subtotal = dto.Items.Sum(i => i.Total);
        var iva = Math.Round(subtotal * TasaIva, 2);
        var total = subtotal + iva;

        entity.ClienteId = dto.ClienteId;
        entity.EmpresaNombreLibre = dto.EmpresaNombreLibre;
        entity.ContactoNombre = dto.ContactoNombre;
        entity.Titulo = dto.Titulo;
        entity.Introduccion = dto.Introduccion;
        entity.AlcanceGeneral = dto.AlcanceGeneral;
        entity.TiempoEntregaDias = dto.TiempoEntregaDias;
        entity.Clausulas = dto.Clausulas;
        entity.ValidezDias = dto.ValidezDias;
        entity.CondicionesPago = dto.CondicionesPago;
        entity.MetodoPago = dto.MetodoPago;
        entity.Subtotal = subtotal;
        entity.Iva = iva;
        entity.Total = total;

        _context.CotizacionItems.RemoveRange(entity.Items);
        entity.Items = dto.Items.Select((i, idx) => new CotizacionItem
        {
            Orden = idx + 1,
            Descripcion = i.Descripcion,
            Cantidad = i.Cantidad,
            Unidad = i.Unidad,
            Total = i.Total
        }).ToList();

        entity.Cliente = dto.ClienteId.HasValue ? await _context.Clientes.FindAsync(dto.ClienteId.Value) : null;

        var empresaTexto = ObtenerEmpresa(entity);
        entity.PdfContenido = Document.Create(container =>
            new CotizacionDocument(entity, empresaTexto).Compose(container))
            .GeneratePdf();

        await _context.SaveChangesAsync();

        return (true, eraBorrador ? "Cotización generada correctamente." : "Cotización actualizada correctamente.", new CotizacionResponseDto
        {
            Id = entity.Id,
            Folio = entity.Folio,
            Estado = entity.Estado,
            Empresa = empresaTexto,
            ContactoNombre = entity.ContactoNombre,
            Titulo = entity.Titulo,
            FechaCotizacion = entity.FechaCotizacion,
            Total = entity.Total
        });
    }

    public async Task<(bool Found, byte[]? Contenido, string Folio)> DescargarAsync(int id)
    {
        var cot = await _context.Cotizaciones.AsNoTracking().FirstOrDefaultAsync(c => c.Id == id);
        return cot is null ? (false, null, "") : (true, cot.PdfContenido, cot.Folio);
    }

    // ─── Borradores (autoguardado) ─────────────────────────────────────────────

    public async Task<(bool Success, string Message, CotizacionResponseDto? Data)> GuardarBorradorNuevoAsync(CotizacionRequestDto dto, int usuarioId)
    {
        var subtotal = dto.Items.Sum(i => i.Total);
        var iva = Math.Round(subtotal * TasaIva, 2);

        var entity = new Cotizacion
        {
            Folio = string.Empty,
            Estado = "Borrador",
            ClienteId = dto.ClienteId,
            EmpresaNombreLibre = dto.EmpresaNombreLibre,
            ContactoNombre = dto.ContactoNombre,
            Titulo = dto.Titulo,
            Introduccion = dto.Introduccion,
            AlcanceGeneral = dto.AlcanceGeneral,
            FechaCotizacion = DateTime.UtcNow.Date,
            TiempoEntregaDias = dto.TiempoEntregaDias,
            Clausulas = dto.Clausulas,
            ValidezDias = dto.ValidezDias,
            CondicionesPago = dto.CondicionesPago,
            MetodoPago = dto.MetodoPago,
            Subtotal = subtotal,
            Iva = iva,
            Total = subtotal + iva,
            CreadoPorId = usuarioId,
            Items = dto.Items.Select((i, idx) => new CotizacionItem
            {
                Orden = idx + 1,
                Descripcion = i.Descripcion,
                Cantidad = i.Cantidad,
                Unidad = i.Unidad,
                Total = i.Total
            }).ToList()
        };

        if (dto.ClienteId.HasValue)
            entity.Cliente = await _context.Clientes.FindAsync(dto.ClienteId.Value);

        _context.Cotizaciones.Add(entity);
        await _context.SaveChangesAsync();

        return (true, "Borrador guardado.", new CotizacionResponseDto
        {
            Id = entity.Id,
            Folio = entity.Folio,
            Estado = entity.Estado,
            Empresa = ObtenerEmpresa(entity),
            ContactoNombre = entity.ContactoNombre,
            Titulo = entity.Titulo,
            FechaCotizacion = entity.FechaCotizacion,
            Total = entity.Total
        });
    }

    public async Task<(bool Success, string Message)> GuardarBorradorExistenteAsync(int id, CotizacionRequestDto dto)
    {
        var entity = await _context.Cotizaciones
            .Include(c => c.Items)
            .FirstOrDefaultAsync(c => c.Id == id);
        if (entity is null) return (false, "Cotización no encontrada.");

        var subtotal = dto.Items.Sum(i => i.Total);
        var iva = Math.Round(subtotal * TasaIva, 2);

        entity.ClienteId = dto.ClienteId;
        entity.EmpresaNombreLibre = dto.EmpresaNombreLibre;
        entity.ContactoNombre = dto.ContactoNombre;
        entity.Titulo = dto.Titulo;
        entity.Introduccion = dto.Introduccion;
        entity.AlcanceGeneral = dto.AlcanceGeneral;
        entity.TiempoEntregaDias = dto.TiempoEntregaDias;
        entity.Clausulas = dto.Clausulas;
        entity.ValidezDias = dto.ValidezDias;
        entity.CondicionesPago = dto.CondicionesPago;
        entity.MetodoPago = dto.MetodoPago;
        entity.Subtotal = subtotal;
        entity.Iva = iva;
        entity.Total = subtotal + iva;

        _context.CotizacionItems.RemoveRange(entity.Items);
        entity.Items = dto.Items.Select((i, idx) => new CotizacionItem
        {
            Orden = idx + 1,
            Descripcion = i.Descripcion,
            Cantidad = i.Cantidad,
            Unidad = i.Unidad,
            Total = i.Total
        }).ToList();

        await _context.SaveChangesAsync();

        return (true, "Borrador guardado.");
    }

    public async Task<(bool Success, string Message)> EliminarAsync(int id)
    {
        var entity = await _context.Cotizaciones.FirstOrDefaultAsync(c => c.Id == id);
        if (entity is null) return (false, "Cotización no encontrada.");

        if (entity.Estado != "Borrador")
            return (false, "Solo se pueden eliminar cotizaciones en borrador; una cotización ya generada no se puede eliminar.");

        _context.Cotizaciones.Remove(entity);
        await _context.SaveChangesAsync();

        return (true, "Borrador eliminado.");
    }
}
