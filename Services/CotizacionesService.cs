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

    public async Task<List<CotizacionResponseDto>> GetAllAsync() =>
        await _context.Cotizaciones
            .AsNoTracking()
            .Include(c => c.Cliente)
            .OrderByDescending(c => c.FechaCreacion)
            .Select(c => new CotizacionResponseDto
            {
                Id = c.Id,
                Folio = c.Folio,
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

        var hoy = DateTime.UtcNow.Date;
        var consecutivo = await _context.Cotizaciones.CountAsync(c => c.FechaCreacion.Date == hoy) + 1;
        var folio = $"BLL{hoy:ddMMyy}{consecutivo}";

        var subtotal = dto.Items.Sum(i => i.Total);
        var iva = Math.Round(subtotal * TasaIva, 2);
        var total = subtotal + iva;

        var entity = new Cotizacion
        {
            Folio = folio,
            ClienteId = dto.ClienteId,
            EmpresaNombreLibre = dto.EmpresaNombreLibre,
            ContactoNombre = dto.ContactoNombre,
            Titulo = dto.Titulo,
            Introduccion = dto.Introduccion,
            AlcanceGeneral = dto.AlcanceGeneral,
            FechaCotizacion = hoy,
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
}
