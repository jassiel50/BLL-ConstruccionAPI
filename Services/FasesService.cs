using System.Security.Claims;
using BLL_ConstruccionAPI.Data;
using BLL_ConstruccionAPI.DTOs.Fases;
using BLL_ConstruccionAPI.DTOs.GastosExtras;
using BLL_ConstruccionAPI.Models.Enums;
using BLL_ConstruccionAPI.Models.Inventario.Proyectos;
using BLL_ConstruccionAPI.Repositories.Interfaces;
using BLL_ConstruccionAPI.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace BLL_ConstruccionAPI.Services;

public class FasesService : IFasesService
{
    private readonly IFasesRepository _fasesRepo;
    private readonly IProyectosRepository _proyectosRepo;
    private readonly AppDbContext _context;
    private readonly IBitacoraService _bitacora;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IEmailService _emailService;
    private readonly IUsuarioRepository _usuarioRepo;

    public FasesService(
        IFasesRepository fasesRepo,
        IProyectosRepository proyectosRepo,
        AppDbContext context,
        IBitacoraService bitacora,
        IHttpContextAccessor httpContextAccessor,
        IEmailService emailService,
        IUsuarioRepository usuarioRepo)
    {
        _fasesRepo = fasesRepo;
        _proyectosRepo = proyectosRepo;
        _context = context;
        _bitacora = bitacora;
        _httpContextAccessor = httpContextAccessor;
        _emailService = emailService;
        _usuarioRepo = usuarioRepo;
    }

    private (int Id, string Nombre, string Ip) GetUsuarioInfo()
    {
        var user = _httpContextAccessor.HttpContext?.User;
        var id = int.TryParse(user?.FindFirstValue(ClaimTypes.NameIdentifier), out var parsed) ? parsed : 0;
        var nombre = user?.FindFirstValue("nombreUsuario") ?? "Sistema";
        var ip = _httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString() ?? "";
        return (id, nombre, ip);
    }

    public async Task<List<FaseResponseDto>> GetByProyectoAsync(int proyectoId)
    {
        var fases = await _fasesRepo.GetByProyectoAsync(proyectoId);
        var faseIds = fases.Select(f => f.Id).ToList();

        var gastosPorFase = await _context.GastosExtras
            .Where(g => faseIds.Contains(g.FaseId))
            .ToListAsync();

        return fases.Select(f =>
        {
            var dto = FaseResponseDto.FromEntity(f);
            var extras = gastosPorFase.Where(g => g.FaseId == f.Id).ToList();
            dto.GastosExtras = extras.Select(g => new GastoExtraDto
            {
                Id = g.Id,
                FaseId = g.FaseId,
                Concepto = g.Concepto,
                Monto = g.Monto,
                Fecha = g.Fecha,
                Observaciones = g.Observaciones
            }).ToList();
            dto.GastoExtra = extras.Sum(g => g.Monto);
            return dto;
        }).ToList();
    }

    public async Task<(bool Success, string Message, FaseResponseDto? Data)> CreateAsync(int proyectoId, FaseRequestDto dto)
    {
        var proyecto = await _proyectosRepo.GetByIdAsync(proyectoId);
        if (proyecto is null || !proyecto.Activo)
            return (false, "El proyecto especificado no existe o está inactivo.", null);

        var fase = new FaseProyecto
        {
            ProyectoId = proyectoId,
            Nombre = dto.Nombre,
            Descripcion = dto.Descripcion,
            Orden = dto.Orden,
            FechaLimite = dto.FechaLimite,
            Estado = EstadoFase.Pendiente,
            FechaRegistro = DateTime.UtcNow
        };

        await _fasesRepo.CreateAsync(fase);

        var (uid, uname, ip) = GetUsuarioInfo();
        await _bitacora.RegistrarAsync(uid, uname, "Creó fase", "FaseProyecto",
            $"Fase '{fase.Nombre}' creada en proyecto ID {proyectoId}.", ip);

        return (true, "Fase creada correctamente.", FaseResponseDto.FromEntity(fase));
    }

    public async Task<(bool Success, string Message)> UpdateAsync(int id, FaseRequestDto dto)
    {
        var fase = await _fasesRepo.GetByIdAsync(id);
        if (fase is null) return (false, "Fase no encontrada.");

        fase.Nombre = dto.Nombre;
        fase.Descripcion = dto.Descripcion;
        fase.Orden = dto.Orden;
        fase.FechaLimite = dto.FechaLimite;

        await _fasesRepo.UpdateAsync(fase);

        var (uid, uname, ip) = GetUsuarioInfo();
        await _bitacora.RegistrarAsync(uid, uname, "Actualizó fase", "FaseProyecto",
            $"Fase '{fase.Nombre}' (ID {fase.Id}) actualizada.", ip);

        return (true, "Fase actualizada correctamente.");
    }

    public async Task<(bool Success, string Message)> CompletarAsync(int id)
    {
        var fase = await _fasesRepo.GetByIdAsync(id);
        if (fase is null) return (false, "Fase no encontrada.");

        if (fase.Estado == EstadoFase.Completada)
            return (false, "La fase ya está completada.");

        fase.Estado = EstadoFase.Completada;
        fase.FechaCompletada = DateTime.UtcNow;

        await _fasesRepo.UpdateAsync(fase);

        var (uid, uname, ip) = GetUsuarioInfo();
        await _bitacora.RegistrarAsync(uid, uname, "Completó fase", "FaseProyecto",
            $"Fase '{fase.Nombre}' (ID {fase.Id}) marcada como completada.", ip);

        return (true, "Fase marcada como completada.");
    }

    public async Task<(bool Success, string Message)> DeleteAsync(int id)
    {
        var fase = await _fasesRepo.GetByIdAsync(id);
        if (fase is null) return (false, "Fase no encontrada.");

        if (fase.Estado == EstadoFase.Completada)
            return (false, "No se puede eliminar una fase ya completada.");

        await _fasesRepo.DeleteAsync(fase);

        var (uid, uname, ip) = GetUsuarioInfo();
        await _bitacora.RegistrarAsync(uid, uname, "Eliminó fase", "FaseProyecto",
            $"Fase '{fase.Nombre}' (ID {fase.Id}) eliminada.", ip);

        return (true, "Fase eliminada correctamente.");
    }

    public async Task<List<FaseResponseDto>> GetAtrasadasAsync()
    {
        var fases = await _fasesRepo.GetAtrasadasAsync();
        return fases.Select(FaseResponseDto.FromEntity).ToList();
    }

    public async Task<List<FaseResponseDto>> GetPorVencerAsync()
    {
        var fases = await _fasesRepo.GetPorVencerAsync();
        return fases.Select(FaseResponseDto.FromEntity).ToList();
    }

    public async Task<(bool Success, string Message, List<FaseResponseDto> Data)> CrearPlaneacionInicialAsync(int proyectoId, PlaneacionInicialRequestDto dto)
    {
        if (dto.Fases is null || !dto.Fases.Any())
            return (false, "Debes proporcionar al menos una fase.", []);

        var proyecto = await _context.Proyectos
            .Include(p => p.Cliente)
            .FirstOrDefaultAsync(p => p.Id == proyectoId);
        if (proyecto is null || !proyecto.Activo)
            return (false, "El proyecto especificado no existe o está inactivo.", []);

        var fasesCreadas = new List<FaseProyecto>();
        foreach (var faseDto in dto.Fases)
        {
            var fase = new FaseProyecto
            {
                ProyectoId   = proyectoId,
                Nombre       = faseDto.Nombre,
                Descripcion  = faseDto.Descripcion,
                Orden        = faseDto.Orden,
                FechaLimite  = faseDto.FechaLimite,
                Estado       = EstadoFase.Pendiente,
                FechaRegistro = DateTime.UtcNow
            };
            await _fasesRepo.CreateAsync(fase);
            fasesCreadas.Add(fase);
        }

        var (uid, uname, ip) = GetUsuarioInfo();
        await _bitacora.RegistrarAsync(uid, uname, "Inició planeación", "FaseProyecto",
            $"{fasesCreadas.Count} fase(s) creadas en proyecto '{proyecto.Nombre}' (ID {proyectoId}).", ip);

        _ = Task.Run(async () =>
        {
            try
            {
                var notificables = await _usuarioRepo.GetUsuariosNotificablesAsync();
                var fasesData = fasesCreadas
                    .OrderBy(f => f.Orden)
                    .Select(f => (f.Nombre, f.Descripcion ?? string.Empty, f.FechaLimite))
                    .ToList();

                foreach (var u in notificables)
                {
                    await _emailService.SendProyectoIniciadoAdminAsync(
                        u.Email, u.Nombre,
                        proyecto.Nombre,
                        proyecto.Cliente?.Nombre ?? "-",
                        proyecto.Ubicacion,
                        proyecto.FechaInicio,
                        proyecto.FechaFin,
                        proyecto.MontoContrato,
                        proyecto.PresupuestoEstimado,
                        proyecto.NumeroCotizacion,
                        proyecto.OrdenCompra,
                        fasesData);
                }
            }
            catch { /* silencioso */ }
        });

        return (true, "Planeación iniciada correctamente.", fasesCreadas.Select(FaseResponseDto.FromEntity).ToList());
    }
}
