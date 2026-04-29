using BLL_ConstruccionAPI.Data;
using BLL_ConstruccionAPI.DTOs.Perdidas;
using BLL_ConstruccionAPI.Models.Enums;
using BLL_ConstruccionAPI.Models.Inventario.Perdidas;
using BLL_ConstruccionAPI.Repositories.Interfaces;
using BLL_ConstruccionAPI.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace BLL_ConstruccionAPI.Services;

public class PerdidasService : IPerdidasService
{
    private readonly AppDbContext _context;
    private readonly IMaterialesRepository _materialesRepo;
    private readonly IHerramientasRepository _herramientasRepo;
    private readonly IBitacoraService _bitacora;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public PerdidasService(
        AppDbContext context,
        IMaterialesRepository materialesRepo,
        IHerramientasRepository herramientasRepo,
        IBitacoraService bitacora,
        IHttpContextAccessor httpContextAccessor)
    {
        _context = context;
        _materialesRepo = materialesRepo;
        _herramientasRepo = herramientasRepo;
        _bitacora = bitacora;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<IEnumerable<RegistroPerdidaResponseDto>> GetAllAsync()
    {
        var registros = await _context.RegistrosPerdidas
            .AsNoTracking()
            .Include(r => r.Material)
            .Include(r => r.Herramienta)
            .Include(r => r.Proyecto)
            .OrderByDescending(r => r.FechaRegistro)
            .ToListAsync();

        return registros.Select(RegistroPerdidaResponseDto.FromEntity);
    }

    public async Task<IEnumerable<RegistroPerdidaResponseDto>> GetByProyectoAsync(int proyectoId)
    {
        var registros = await _context.RegistrosPerdidas
            .AsNoTracking()
            .Include(r => r.Material)
            .Include(r => r.Herramienta)
            .Include(r => r.Proyecto)
            .Where(r => r.ProyectoId == proyectoId)
            .OrderByDescending(r => r.FechaRegistro)
            .ToListAsync();

        return registros.Select(RegistroPerdidaResponseDto.FromEntity);
    }

    public async Task<IEnumerable<RegistroPerdidaResponseDto>> GetByMaterialAsync(int materialId)
    {
        var registros = await _context.RegistrosPerdidas
            .AsNoTracking()
            .Include(r => r.Material)
            .Include(r => r.Herramienta)
            .Include(r => r.Proyecto)
            .Where(r => r.MaterialId == materialId)
            .OrderByDescending(r => r.FechaRegistro)
            .ToListAsync();

        return registros.Select(RegistroPerdidaResponseDto.FromEntity);
    }

    public async Task<IEnumerable<RegistroPerdidaResponseDto>> GetByHerramientaAsync(int herramientaId)
    {
        var registros = await _context.RegistrosPerdidas
            .AsNoTracking()
            .Include(r => r.Material)
            .Include(r => r.Herramienta)
            .Include(r => r.Proyecto)
            .Where(r => r.HerramientaId == herramientaId)
            .OrderByDescending(r => r.FechaRegistro)
            .ToListAsync();

        return registros.Select(RegistroPerdidaResponseDto.FromEntity);
    }

    public async Task<RegistroPerdidaResponseDto?> GetByIdAsync(int id)
    {
        var registro = await _context.RegistrosPerdidas
            .AsNoTracking()
            .Include(r => r.Material)
            .Include(r => r.Herramienta)
            .Include(r => r.Proyecto)
            .FirstOrDefaultAsync(r => r.Id == id);

        return registro is null ? null : RegistroPerdidaResponseDto.FromEntity(registro);
    }

    public async Task<(bool Success, string Message, RegistroPerdidaResponseDto? Data)> CreateAsync(int usuarioId, RegistroPerdidaRequestDto dto)
    {
        // Validar Tipo
        if (!Enum.TryParse<TipoPerdida>(dto.Tipo, ignoreCase: true, out var tipo))
            return (false, $"Tipo de pérdida inválido: '{dto.Tipo}'. Valores válidos: Material, Herramienta.", null);

        // Validar Motivo
        if (!Enum.TryParse<MotivoPerdida>(dto.Motivo, ignoreCase: true, out var motivo))
            return (false, $"Motivo inválido: '{dto.Motivo}'. Valores válidos: Robo, Daño, Extravío, Destruccion, Otro.", null);

        var registro = new RegistroPerdida
        {
            Tipo              = tipo,
            Motivo            = motivo,
            ProyectoId        = dto.ProyectoId,
            UsuarioReportaId  = usuarioId,
            Descripcion       = dto.Descripcion,
            FechaPerdida      = dto.FechaPerdida,
            FechaRegistro     = DateTime.UtcNow
        };

        if (tipo == TipoPerdida.Material)
        {
            if (dto.MaterialId is null)
                return (false, "MaterialId es requerido cuando el tipo es Material.", null);

            var almacen = await _materialesRepo.GetStockCentralAsync(dto.MaterialId.Value);
            if (almacen is null)
                return (false, "No se encontró el material o su registro de stock.", null);

            if (dto.CantidadPerdida <= 0)
                return (false, "La cantidad perdida debe ser mayor a cero.", null);

            if (almacen.Stock - dto.CantidadPerdida < 0)
                return (false, $"Stock insuficiente. Stock actual: {almacen.Stock}, cantidad a registrar: {dto.CantidadPerdida}.", null);

            almacen.Stock -= dto.CantidadPerdida;
            await _materialesRepo.UpdateStockCentralAsync(almacen);

            registro.MaterialId      = dto.MaterialId;
            registro.CantidadPerdida = dto.CantidadPerdida;
        }
        else // Herramienta
        {
            if (dto.HerramientaId is null)
                return (false, "HerramientaId es requerido cuando el tipo es Herramienta.", null);

            var herramienta = await _herramientasRepo.GetByIdAsync(dto.HerramientaId.Value);
            if (herramienta is null || !herramienta.Activo)
                return (false, "Herramienta no encontrada o inactiva.", null);

            herramienta.Estado = EstadoHerramienta.Baja;
            await _herramientasRepo.UpdateAsync(herramienta);

            registro.HerramientaId   = dto.HerramientaId;
            registro.CantidadPerdida = 0;
        }

        _context.RegistrosPerdidas.Add(registro);
        await _context.SaveChangesAsync();

        // Recargar con navegación para el response
        var result = await _context.RegistrosPerdidas
            .AsNoTracking()
            .Include(r => r.Material)
            .Include(r => r.Herramienta)
            .Include(r => r.Proyecto)
            .FirstAsync(r => r.Id == registro.Id);

        var tipoBitacora = result.Tipo.ToString();
        var nombreBitacora = result.Material?.Nombre ?? result.Herramienta?.Nombre ?? "N/A";
        var motivoBitacora = result.Motivo.ToString();
        var (uid, uname) = GetUsuarioInfo();
        await _bitacora.RegistrarAsync(uid, uname, "Registró", "Pérdida", $"Pérdida de {tipoBitacora}: '{nombreBitacora}', motivo: {motivoBitacora}");

        return (true, "Pérdida registrada correctamente.", RegistroPerdidaResponseDto.FromEntity(result));
    }

    public async Task<(bool Success, string Message)> DeleteAsync(int id)
    {
        var registro = await _context.RegistrosPerdidas.FirstOrDefaultAsync(r => r.Id == id);
        if (registro is null)
            return (false, "Registro de pérdida no encontrado.");

        _context.RegistrosPerdidas.Remove(registro);
        await _context.SaveChangesAsync();

        var (uid, uname) = GetUsuarioInfo();
        await _bitacora.RegistrarAsync(uid, uname, "Eliminó", "Pérdida", $"Registro de pérdida #{id} eliminado");
        return (true, "Registro de pérdida eliminado correctamente.");
    }

    private (int UsuarioId, string NombreUsuario) GetUsuarioInfo()
    {
        var user = _httpContextAccessor.HttpContext?.User;
        var id = int.TryParse(user?.FindFirstValue(ClaimTypes.NameIdentifier), out var parsed) ? parsed : 0;
        var nombre = user?.FindFirstValue("nombreUsuario") ?? "Sistema";
        return (id, nombre);
    }
}
