using System.Security.Claims;
using BLL_ConstruccionAPI.Data;
using BLL_ConstruccionAPI.DTOs.Proyectos;
using BLL_ConstruccionAPI.Models.Inventario.Proyectos;
using BLL_ConstruccionAPI.Reports;
using BLL_ConstruccionAPI.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Fluent;

namespace BLL_ConstruccionAPI.Services;

public class RequisicionMaterialService : IRequisicionMaterialService
{
    private readonly AppDbContext _context;
    private readonly IBitacoraService _bitacora;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public RequisicionMaterialService(AppDbContext context, IBitacoraService bitacora, IHttpContextAccessor httpContextAccessor)
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

    private static RequisicionMaterialDto MapToDto(RequisicionMaterial r) => new()
    {
        Id = r.Id,
        ProyectoId = r.ProyectoId,
        ProyectoNombre = r.Proyecto?.Nombre ?? string.Empty,
        Folio = r.Folio,
        SeRequierePara = r.SeRequierePara,
        SeSuministraPor = r.SeSuministraPor,
        FechaSolicitud = r.FechaSolicitud,
        SolicitoNombre = r.SolicitoNombre,
        FechaCreacion = r.FechaCreacion,
        FaseId = r.FaseId,
        FaseNombre = r.Fase?.Nombre,
        Detalles = r.Detalles
            .OrderBy(d => d.Orden)
            .Select(d => new RequisicionMaterialDetalleDto
            {
                Id = d.Id,
                Orden = d.Orden,
                Descripcion = d.Descripcion,
                Unidad = d.Unidad,
                Cantidad = d.Cantidad,
                AreaComentarios = d.AreaComentarios,
                Status = d.Status,
                MaterialId = d.MaterialId,
                Responsable = d.Responsable,
                CostoUnitario = d.CostoUnitario,
                GastoExtraId = d.GastoExtraId
            }).ToList()
    };

    public async Task<List<RequisicionMaterialDto>> GetByProyectoAsync(int proyectoId)
    {
        var lista = await _context.RequisicionesMaterial
            .AsNoTracking()
            .Include(r => r.Proyecto)
            .Include(r => r.Fase)
            .Include(r => r.Detalles)
            .Where(r => r.ProyectoId == proyectoId)
            .OrderByDescending(r => r.FechaCreacion)
            .ToListAsync();

        return lista.Select(MapToDto).ToList();
    }

    public async Task<RequisicionMaterialDto?> GetByIdAsync(int id)
    {
        var entity = await _context.RequisicionesMaterial
            .AsNoTracking()
            .Include(r => r.Proyecto)
            .Include(r => r.Fase)
            .Include(r => r.Detalles)
            .FirstOrDefaultAsync(r => r.Id == id);

        return entity is null ? null : MapToDto(entity);
    }

    public async Task<(bool Success, string Message, RequisicionMaterialDto? Data)> CreateAsync(int proyectoId, RequisicionMaterialRequestDto dto)
    {
        var proyecto = await _context.Proyectos.FirstOrDefaultAsync(p => p.Id == proyectoId);
        if (proyecto is null) return (false, "Proyecto no encontrado.", null);

        if (dto.Detalles.Count == 0)
            return (false, "Agrega al menos un renglón a la requisición.", null);

        if (dto.Detalles.Any(d => string.IsNullOrWhiteSpace(d.Descripcion)))
            return (false, "Todos los renglones deben tener descripción.", null);

        var tieneRenglonesEmpresa = dto.Detalles.Any(d => d.Responsable == "Empresa" && d.CostoUnitario > 0);
        if (tieneRenglonesEmpresa && dto.FaseId is null)
            return (false, "Selecciona la fase del proyecto para registrar el gasto de los materiales que le tocan a la empresa.", null);

        if (dto.FaseId.HasValue)
        {
            var faseValida = await _context.FaseProyectos.AnyAsync(f => f.Id == dto.FaseId.Value && f.ProyectoId == proyectoId);
            if (!faseValida) return (false, "La fase seleccionada no pertenece a este proyecto.", null);
        }

        var (uid, uname, ip) = GetUsuarioInfo();

        var siguienteFolio = await _context.RequisicionesMaterial
            .Where(r => r.ProyectoId == proyectoId)
            .Select(r => (int?)r.Folio)
            .MaxAsync() ?? 0;

        var entity = new RequisicionMaterial
        {
            ProyectoId = proyectoId,
            Folio = siguienteFolio + 1,
            SeRequierePara = dto.SeRequierePara.Trim(),
            SeSuministraPor = dto.SeSuministraPor.Trim(),
            FechaSolicitud = dto.FechaSolicitud,
            SolicitoNombre = string.IsNullOrWhiteSpace(dto.SolicitoNombre) ? uname : dto.SolicitoNombre.Trim(),
            CreadoPorId = uid,
            FechaCreacion = DateTime.UtcNow,
            FaseId = dto.FaseId,
            Detalles = dto.Detalles.Select((d, i) => new RequisicionMaterialDetalle
            {
                Orden = i + 1,
                Descripcion = d.Descripcion.Trim(),
                Unidad = d.Unidad.Trim(),
                Cantidad = d.Cantidad,
                AreaComentarios = d.AreaComentarios,
                Status = string.IsNullOrWhiteSpace(d.Status) ? "Pendiente" : d.Status.Trim(),
                MaterialId = d.MaterialId,
                Responsable = d.Responsable == "Empresa" ? "Empresa" : "Cliente",
                CostoUnitario = d.CostoUnitario
            }).ToList()
        };

        _context.RequisicionesMaterial.Add(entity);
        await _context.SaveChangesAsync();

        // Los renglones a cargo de la empresa generan su gasto extra en la fase indicada,
        // para que aparezcan en el panel financiero del proyecto (solo lo que nos toca poner a nosotros).
        foreach (var detalle in entity.Detalles.Where(d => d.Responsable == "Empresa" && d.CostoUnitario > 0 && d.Cantidad > 0))
        {
            var gastoExtra = new GastoExtra
            {
                FaseId = dto.FaseId!.Value,
                Concepto = $"Material (Requisición #{entity.Folio}): {detalle.Descripcion}",
                Monto = detalle.CostoUnitario * detalle.Cantidad,
                Categoria = "Material",
                Fecha = entity.FechaSolicitud,
                Observaciones = $"Generado automáticamente desde la requisición de materiales #{entity.Folio}.",
                FechaRegistro = DateTime.UtcNow
            };
            _context.GastosExtras.Add(gastoExtra);
            await _context.SaveChangesAsync();
            detalle.GastoExtraId = gastoExtra.Id;
        }
        await _context.SaveChangesAsync();

        await _bitacora.RegistrarAsync(uid, uname, "Creó requisición de materiales", "RequisicionMaterial",
            $"Requisición #{entity.Folio} ({entity.Detalles.Count} renglón(es)) creada en proyecto '{proyecto.Nombre}' (ID {proyectoId}).", ip);

        entity.Proyecto = proyecto;
        return (true, "Requisición creada.", MapToDto(entity));
    }

    public async Task<(bool Success, string Message)> DeleteAsync(int id)
    {
        var entity = await _context.RequisicionesMaterial
            .Include(r => r.Detalles)
            .FirstOrDefaultAsync(r => r.Id == id);
        if (entity is null) return (false, "Requisición no encontrada.");

        var gastoExtraIds = entity.Detalles.Where(d => d.GastoExtraId.HasValue).Select(d => d.GastoExtraId!.Value).ToList();
        if (gastoExtraIds.Count > 0)
        {
            var gastos = await _context.GastosExtras.Where(g => gastoExtraIds.Contains(g.Id)).ToListAsync();
            _context.GastosExtras.RemoveRange(gastos);
        }

        _context.RequisicionesMaterial.Remove(entity);
        await _context.SaveChangesAsync();

        var (uid, uname, ip) = GetUsuarioInfo();
        await _bitacora.RegistrarAsync(uid, uname, "Eliminó requisición de materiales", "RequisicionMaterial",
            $"Requisición #{entity.Folio} (ID {id}) eliminada.", ip);

        return (true, "Requisición eliminada.");
    }

    public async Task<(bool Success, string Message, byte[]? Pdf)> GenerarPdfAsync(int id)
    {
        var entity = await _context.RequisicionesMaterial
            .AsNoTracking()
            .Include(r => r.Proyecto)
            .Include(r => r.Detalles)
            .FirstOrDefaultAsync(r => r.Id == id);

        if (entity is null) return (false, "Requisición no encontrada.", null);

        var documento = new RequisicionMaterialDocument(entity);
        var pdf = documento.GeneratePdf();
        return (true, "OK", pdf);
    }
}
