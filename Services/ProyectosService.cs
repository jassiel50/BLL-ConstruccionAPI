using BLL_ConstruccionAPI.Data;
using BLL_ConstruccionAPI.DTOs.Herramientas;
using BLL_ConstruccionAPI.DTOs.Materiales;
using BLL_ConstruccionAPI.DTOs.Proyectos;
using BLL_ConstruccionAPI.Models.Enums;
using BLL_ConstruccionAPI.Models.Inventario.Proyectos;
using BLL_ConstruccionAPI.Repositories.Interfaces;
using BLL_ConstruccionAPI.Reports;
using BLL_ConstruccionAPI.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Fluent;
using System.Security.Claims;

namespace BLL_ConstruccionAPI.Services;

public class ProyectosService : IProyectosService
{
    private readonly IProyectosRepository _proyectosRepo;
    private readonly IProveedoresClientesRepository _provClientesRepo;
    private readonly ISalidasRepository _salidasRepo;
    private readonly IBitacoraService _bitacora;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly AppDbContext _context;
    private readonly IEmailService _emailService;
    private readonly IUsuarioRepository _usuarioRepo;

    private static readonly EstadoProyecto[] EstadosValidos =
        [EstadoProyecto.Activo, EstadoProyecto.Pausado, EstadoProyecto.Terminado];

    public ProyectosService(
        IProyectosRepository proyectosRepo,
        IProveedoresClientesRepository provClientesRepo,
        ISalidasRepository salidasRepo,
        IBitacoraService bitacora,
        IHttpContextAccessor httpContextAccessor,
        AppDbContext context,
        IEmailService emailService,
        IUsuarioRepository usuarioRepo)
    {
        _proyectosRepo = proyectosRepo;
        _provClientesRepo = provClientesRepo;
        _salidasRepo = salidasRepo;
        _bitacora = bitacora;
        _httpContextAccessor = httpContextAccessor;
        _context = context;
        _emailService = emailService;
        _usuarioRepo = usuarioRepo;
    }

    public async Task<IEnumerable<ProyectoResponseDto>> GetAllAsync()
    {
        var proyectos = await _proyectosRepo.GetAllAsync();
        return proyectos.Select(ProyectoResponseDto.FromEntity);
    }

    public async Task<IEnumerable<ProyectoResponseDto>> GetByClienteAsync(int clienteId)
    {
        var proyectos = await _proyectosRepo.GetByClienteAsync(clienteId);
        return proyectos.Select(ProyectoResponseDto.FromEntity);
    }

    public async Task<ProyectoResponseDto?> GetByIdAsync(int id)
    {
        var proyecto = await _proyectosRepo.GetByIdAsync(id);
        if (proyecto is null) return null;

        var dto = ProyectoResponseDto.FromEntity(proyecto);

        // GastoMateriales: sum of Cantidad * PrecioUnitario stored at salida time
        var salidas = await _context.Salidas
            .Include(s => s.Detalles)
            .Where(s => s.ProyectoId == id)
            .ToListAsync();

        var gastoMateriales = salidas
            .SelectMany(s => s.Detalles)
            .Sum(d => d.Cantidad * d.PrecioUnitario);

        // GastoHerramientas: sum of ValorAdquisicion for all assigned herramientas
        var gastoHerramientas = await _context.AsignacionesHerramienta
            .Include(a => a.Herramienta)
            .Where(a => a.ProyectoId == id)
            .SumAsync(a => a.Herramienta!.ValorAdquisicion);

        // GastoExtras: sum of all GastosExtras of all fases of this project
        var faseIds = await _context.FaseProyectos
            .Where(f => f.ProyectoId == id)
            .Select(f => f.Id)
            .ToListAsync();

        var gastoExtras = faseIds.Any()
            ? await _context.GastosExtras
                .Where(g => faseIds.Contains(g.FaseId))
                .SumAsync(g => g.Monto)
            : 0;

        var gastoReal = gastoMateriales + gastoHerramientas + gastoExtras;
        dto.GastoMateriales = gastoMateriales;
        dto.GastoHerramientas = gastoHerramientas;
        dto.GastoExtras = gastoExtras;
        dto.GastoReal = gastoReal;
        dto.Utilidad = dto.MontoContrato - gastoReal;
        dto.Varianza = dto.PresupuestoEstimado - gastoReal;
        dto.SobrepasadoPresupuesto = dto.PresupuestoEstimado > 0 && gastoReal > dto.PresupuestoEstimado;
        dto.SobrepasadoContrato = dto.MontoContrato > 0 && gastoReal > dto.MontoContrato;

        return dto;
    }

    public async Task<(bool Success, string Message, ProyectoResponseDto? Data)> CreateAsync(ProyectoRequestDto dto)
    {
        if (!Enum.TryParse<EstadoProyecto>(dto.Estado, out var estadoProyecto))
            return (false, $"Estado inválido. Los valores permitidos son: {string.Join(", ", Enum.GetNames<EstadoProyecto>())}.", null);

        var cliente = await _provClientesRepo.GetClienteByIdAsync(dto.ClienteId);
        if (cliente is null || !cliente.Activo)
            return (false, "El cliente especificado no existe o está inactivo.", null);

        var proyecto = new Proyecto
        {
            Nombre = dto.Nombre,
            Descripcion = dto.Descripcion,
            Ubicacion = dto.Ubicacion,
            ClienteId = dto.ClienteId,
            ResponsableId = dto.ResponsableId,
            FechaInicio = dto.FechaInicio,
            FechaFin = dto.FechaFin,
            Estado = estadoProyecto,
            FechaRegistro = DateTime.UtcNow,
            Cliente = cliente,
            NumeroCotizacion = dto.NumeroCotizacion,
            OrdenCompra = dto.OrdenCompra,
            MontoContrato = dto.MontoContrato,
            PresupuestoEstimado = dto.PresupuestoEstimado
        };

        await _proyectosRepo.CreateAsync(proyecto);
        var (uid, uname) = GetUsuarioInfo();
        await _bitacora.RegistrarAsync(uid, uname, "Creó", "Proyecto", $"Proyecto '{proyecto.Nombre}' creado");
        return (true, "Proyecto creado correctamente.", ProyectoResponseDto.FromEntity(proyecto));
    }

    public async Task<(bool Success, string Message)> UpdateAsync(int id, ProyectoRequestDto dto)
    {
        var proyecto = await _proyectosRepo.GetByIdAsync(id);
        if (proyecto is null) return (false, "Proyecto no encontrado.");

        if (!Enum.TryParse<EstadoProyecto>(dto.Estado, out var estadoActualizado))
            return (false, $"Estado inválido. Los valores permitidos son: {string.Join(", ", Enum.GetNames<EstadoProyecto>())}.");

        if (proyecto.ClienteId != dto.ClienteId)
        {
            var cliente = await _provClientesRepo.GetClienteByIdAsync(dto.ClienteId);
            if (cliente is null || !cliente.Activo)
                return (false, "El cliente especificado no existe o está inactivo.");
        }

        proyecto.Nombre = dto.Nombre;
        proyecto.Descripcion = dto.Descripcion;
        proyecto.Ubicacion = dto.Ubicacion;
        proyecto.ClienteId = dto.ClienteId;
        proyecto.ResponsableId = dto.ResponsableId;
        proyecto.FechaInicio = dto.FechaInicio;
        proyecto.FechaFin = dto.FechaFin;
        proyecto.Estado = estadoActualizado;
        proyecto.NumeroCotizacion = dto.NumeroCotizacion;
        proyecto.OrdenCompra = dto.OrdenCompra;
        proyecto.MontoContrato = dto.MontoContrato;
        proyecto.PresupuestoEstimado = dto.PresupuestoEstimado;

        await _proyectosRepo.UpdateAsync(proyecto);
        var (uid2, uname2) = GetUsuarioInfo();
        await _bitacora.RegistrarAsync(uid2, uname2, "Actualizó", "Proyecto", $"Proyecto '{proyecto.Nombre}' actualizado");
        return (true, "Proyecto actualizado correctamente.");
    }

    public async Task<(bool Success, string Message)> DeleteAsync(int id)
    {
        var proyecto = await _proyectosRepo.GetByIdAsync(id);
        if (proyecto is null) return (false, "Proyecto no encontrado.");

        await _proyectosRepo.DeleteAsync(proyecto);
        var (uid, uname) = GetUsuarioInfo();
        await _bitacora.RegistrarAsync(uid, uname, "Eliminó", "Proyecto", $"Proyecto '{proyecto.Nombre}' eliminado");
        return (true, "Proyecto eliminado correctamente.");
    }

    public async Task<(bool Success, string Message)> TerminarAsync(int id)
    {
        var proyecto = await _proyectosRepo.GetByIdAsync(id);
        if (proyecto is null) return (false, "Proyecto no encontrado.");

        if (proyecto.Estado == EstadoProyecto.Terminado)
            return (false, "El proyecto ya está marcado como Terminado.");

        await _proyectosRepo.TerminarAsync(proyecto);
        var (uid2, uname2) = GetUsuarioInfo();
        await _bitacora.RegistrarAsync(uid2, uname2, "Finalizó", "Proyecto", $"Proyecto '{proyecto.Nombre}' marcado como Terminado");
        return (true, "Proyecto marcado como Terminado correctamente.");
    }

    public async Task<IEnumerable<AlmacenProyectoResponseDto>> GetMaterialesAsync(int proyectoId)
    {
        var materiales = await _proyectosRepo.GetMaterialesAsync(proyectoId);
        return materiales.Select(AlmacenProyectoResponseDto.FromEntity);
    }

    public async Task<IEnumerable<AsignacionHerramientaResponseDto>> GetHerramientasAsync(int proyectoId)
    {
        var herramientas = await _proyectosRepo.GetHerramientasActivasAsync(proyectoId);
        return herramientas.Select(AsignacionHerramientaResponseDto.FromEntity);
    }

    public async Task<(bool Success, string Message, int Count)> DevolverTodasHerramientasAsync(int proyectoId)
    {
        var proyecto = await _proyectosRepo.GetByIdAsync(proyectoId);
        if (proyecto is null) return (false, "Proyecto no encontrado.", 0);

        var count = await _proyectosRepo.DevolverTodasHerramientasAsync(proyectoId);

        var (uid, uname) = GetUsuarioInfo();
        await _bitacora.RegistrarAsync(uid, uname, "Devolvió herramientas", "Proyecto",
            $"{count} herramienta(s) devueltas del proyecto '{proyecto.Nombre}'");

        return (true, $"{count} herramienta(s) devuelta(s) correctamente.", count);
    }

    public async Task<(bool Success, string Message, byte[]? Pdf)> GenerarPlaneacionAsync(int proyectoId)
    {
        var proyecto = await _proyectosRepo.GetByIdAsync(proyectoId);
        if (proyecto is null || !proyecto.Activo)
            return (false, "Proyecto no encontrado.", null);

        var fases = await _context.FaseProyectos
            .Where(f => f.ProyectoId == proyectoId)
            .OrderBy(f => f.Orden)
            .ToListAsync();

        if (!fases.Any())
            return (false, "El proyecto no tiene fases registradas. Agrega las fases antes de generar la planeación.", null);

        var pdfBytes = Document.Create(container =>
            new PlaneacionDocument(proyecto, fases).Compose(container)).GeneratePdf();

        _ = Task.Run(async () =>
        {
            try
            {
                var notificables = await _usuarioRepo.GetUsuariosNotificablesAsync();
                var fasesData = fases.Select(f => (f.Nombre, f.Descripcion, f.FechaLimite)).ToList();
                foreach (var u in notificables)
                {
                    await _emailService.SendProyectoIniciadoAdminAsync(
                        u.Email, u.Nombre,
                        proyecto.Nombre, proyecto.Cliente?.Nombre ?? "-",
                        proyecto.Ubicacion, proyecto.FechaInicio, proyecto.FechaFin,
                        proyecto.MontoContrato, proyecto.PresupuestoEstimado,
                        proyecto.NumeroCotizacion, proyecto.OrdenCompra,
                        fasesData);
                }
            }
            catch { /* silencioso */ }
        });

        return (true, "Planeación generada correctamente.", pdfBytes);
    }

    private (int UsuarioId, string NombreUsuario) GetUsuarioInfo()
    {
        var user = _httpContextAccessor.HttpContext?.User;
        var id = int.TryParse(user?.FindFirstValue(ClaimTypes.NameIdentifier), out var parsed) ? parsed : 0;
        var nombre = user?.FindFirstValue("nombreUsuario") ?? "Sistema";
        return (id, nombre);
    }

    public async Task<(bool Success, string Message, byte[]? Pdf)> GenerarPlaneacionAsync(int proyectoId)
    {
        var proyecto = await _proyectosRepo.GetByIdAsync(proyectoId);
        if (proyecto is null || !proyecto.Activo)
            return (false, "Proyecto no encontrado.", null);

        var fases = await _context.FaseProyectos
            .Where(f => f.ProyectoId == proyectoId)
            .OrderBy(f => f.Orden)
            .ToListAsync();

        if (!fases.Any())
            return (false, "El proyecto no tiene fases registradas. Agrega las fases antes de generar la planeación.", null);

        var pdfBytes = Document.Create(container => new PlaneacionDocument(proyecto, fases).Compose(container)).GeneratePdf();

        _ = Task.Run(async () =>
        {
            try
            {
                var notificables = await _usuarioRepo.GetUsuariosNotificablesAsync();
                var fasesData = fases.Select(f => (f.Nombre, f.Descripcion, f.FechaLimite)).ToList();
                foreach (var u in notificables)
                {
                    await _emailService.SendProyectoIniciadoAdminAsync(
                        u.Email, u.Nombre,
                        proyecto.Nombre, proyecto.Cliente?.Nombre ?? "-",
                        proyecto.Ubicacion, proyecto.FechaInicio, proyecto.FechaFin,
                        proyecto.MontoContrato, proyecto.PresupuestoEstimado,
                        proyecto.NumeroCotizacion, proyecto.OrdenCompra,
                        fasesData);
                }
            }
            catch { /* silencioso */ }
        });

        return (true, "Planeación generada correctamente.", pdfBytes);
    }
}
