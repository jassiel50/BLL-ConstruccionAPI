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

    private static readonly EstadoProyecto[] EstadosValidos =
        [EstadoProyecto.Activo, EstadoProyecto.Pausado, EstadoProyecto.Terminado, EstadoProyecto.Archivado];

    public ProyectosService(
        IProyectosRepository proyectosRepo,
        IProveedoresClientesRepository provClientesRepo,
        ISalidasRepository salidasRepo,
        IBitacoraService bitacora,
        IHttpContextAccessor httpContextAccessor,
        AppDbContext context)
    {
        _proyectosRepo = proyectosRepo;
        _provClientesRepo = provClientesRepo;
        _salidasRepo = salidasRepo;
        _bitacora = bitacora;
        _httpContextAccessor = httpContextAccessor;
        _context = context;
    }

    public async Task<IEnumerable<ProyectoResponseDto>> GetAllAsync()
    {
        var proyectos = await _proyectosRepo.GetAllAsync();
        var dtos = proyectos.Select(ProyectoResponseDto.FromEntity).ToList();
        await AplicarGastosAsync(dtos);
        return dtos;
    }

    public async Task<IEnumerable<ProyectoResponseDto>> GetByClienteAsync(int clienteId)
    {
        var proyectos = await _proyectosRepo.GetByClienteAsync(clienteId);
        var dtos = proyectos.Select(ProyectoResponseDto.FromEntity).ToList();
        await AplicarGastosAsync(dtos);
        return dtos;
    }

    public async Task<ProyectoResponseDto?> GetByIdAsync(int id)
    {
        var proyecto = await _proyectosRepo.GetByIdAsync(id);
        if (proyecto is null) return null;

        var dto = ProyectoResponseDto.FromEntity(proyecto);
        await AplicarGastosAsync([dto]);
        return dto;
    }

    /// <summary>
    /// Calcula en bloque GastoMateriales/GastoHerramientas/GastoExtras/GastoReal/Utilidad/Varianza
    /// y las banderas de sobrepasado para una lista de proyectos, usando agregaciones agrupadas
    /// (en vez de una consulta por proyecto) para que también sea eficiente en listados.
    /// </summary>
    private async Task AplicarGastosAsync(List<ProyectoResponseDto> dtos)
    {
        if (dtos.Count == 0) return;
        var proyectoIds = dtos.Select(d => d.Id).ToList();

        var gastoMaterialesPorProyecto = await _context.Salidas
            .Where(s => proyectoIds.Contains(s.ProyectoId))
            .SelectMany(s => s.Detalles.Select(d => new { s.ProyectoId, Monto = d.Cantidad * d.PrecioUnitario }))
            .GroupBy(x => x.ProyectoId)
            .Select(g => new { ProyectoId = g.Key, Total = g.Sum(x => x.Monto) })
            .ToDictionaryAsync(g => g.ProyectoId, g => g.Total);

        var gastoHerramientasPorProyecto = await _context.AsignacionesHerramienta
            .Where(a => proyectoIds.Contains(a.ProyectoId))
            .GroupBy(a => a.ProyectoId)
            .Select(g => new { ProyectoId = g.Key, Total = g.Sum(a => a.Herramienta!.ValorAdquisicion) })
            .ToDictionaryAsync(g => g.ProyectoId, g => g.Total);

        var fasesPorProyecto = await _context.FaseProyectos
            .Where(f => proyectoIds.Contains(f.ProyectoId))
            .Select(f => new { f.Id, f.ProyectoId })
            .ToListAsync();
        var faseIds = fasesPorProyecto.Select(f => f.Id).ToList();

        var gastoExtrasPorFase = faseIds.Count > 0
            ? await _context.GastosExtras
                .Where(g => faseIds.Contains(g.FaseId))
                .GroupBy(g => g.FaseId)
                .Select(g => new { FaseId = g.Key, Total = g.Sum(x => x.Monto) })
                .ToDictionaryAsync(g => g.FaseId, g => g.Total)
            : [];

        var gastoExtrasPorProyecto = fasesPorProyecto
            .GroupBy(f => f.ProyectoId)
            .ToDictionary(g => g.Key, g => g.Sum(f => gastoExtrasPorFase.GetValueOrDefault(f.Id)));

        foreach (var dto in dtos)
        {
            var gastoMateriales = gastoMaterialesPorProyecto.GetValueOrDefault(dto.Id);
            var gastoHerramientas = gastoHerramientasPorProyecto.GetValueOrDefault(dto.Id);
            var gastoExtras = gastoExtrasPorProyecto.GetValueOrDefault(dto.Id);
            var gastoReal = gastoMateriales + gastoHerramientas + gastoExtras;

            dto.GastoMateriales = gastoMateriales;
            dto.GastoHerramientas = gastoHerramientas;
            dto.GastoExtras = gastoExtras;
            dto.GastoReal = gastoReal;
            dto.Utilidad = dto.MontoContrato - gastoReal;
            dto.Varianza = dto.PresupuestoEstimado - gastoReal;
            dto.SobrepasadoPresupuesto = dto.PresupuestoEstimado > 0 && gastoReal > dto.PresupuestoEstimado;
            dto.SobrepasadoContrato = dto.MontoContrato > 0 && gastoReal > dto.MontoContrato;
        }
    }

    public async Task<List<HistorialFinancieroItemDto>> GetHistorialFinancieroAsync(int proyectoId)
    {
        var items = new List<HistorialFinancieroItemDto>();

        // Ingresos: pagos de cliente
        var pagos = await _context.PagosCliente
            .AsNoTracking()
            .Where(p => p.ProyectoId == proyectoId)
            .ToListAsync();
        items.AddRange(pagos.Select(p => new HistorialFinancieroItemDto
        {
            Fecha = p.FechaPago,
            Tipo = "Ingreso",
            Categoria = "Pago",
            Concepto = p.Concepto,
            Monto = p.Monto,
            Referencia = string.IsNullOrWhiteSpace(p.NumeroFactura) ? null : p.NumeroFactura
        }));

        // Gastos: salidas de materiales
        var salidas = await _context.Salidas
            .AsNoTracking()
            .Include(s => s.Detalles)
            .Where(s => s.ProyectoId == proyectoId)
            .ToListAsync();
        items.AddRange(salidas
            .Where(s => s.Detalles.Count > 0)
            .Select(s => new HistorialFinancieroItemDto
            {
                Fecha = s.Fecha,
                Tipo = "Gasto",
                Categoria = "Material",
                Concepto = $"Salida de materiales ({s.Detalles.Count} artículo{(s.Detalles.Count == 1 ? "" : "s")})",
                Monto = s.Detalles.Sum(d => d.Cantidad * d.PrecioUnitario),
                Referencia = s.NumeroFolio
            }));

        // Gastos: herramientas asignadas (costo de adquisición, al momento de asignarse)
        var asignaciones = await _context.AsignacionesHerramienta
            .AsNoTracking()
            .Include(a => a.Herramienta)
            .Where(a => a.ProyectoId == proyectoId)
            .ToListAsync();
        items.AddRange(asignaciones.Select(a => new HistorialFinancieroItemDto
        {
            Fecha = a.FechaAsignacion,
            Tipo = "Gasto",
            Categoria = "Herramienta",
            Concepto = $"Asignación: {a.Herramienta?.Nombre ?? "Herramienta"}",
            Monto = a.Herramienta?.ValorAdquisicion ?? 0,
            Referencia = a.Herramienta?.Codigo
        }));

        // Gastos: extras por fase
        var faseIds = await _context.FaseProyectos
            .Where(f => f.ProyectoId == proyectoId)
            .Select(f => f.Id)
            .ToListAsync();
        var gastosExtras = faseIds.Count > 0
            ? await _context.GastosExtras.AsNoTracking().Where(g => faseIds.Contains(g.FaseId)).ToListAsync()
            : [];
        items.AddRange(gastosExtras.Select(g => new HistorialFinancieroItemDto
        {
            Fecha = g.Fecha,
            Tipo = "Gasto",
            Categoria = "Gasto Extra",
            Concepto = g.Concepto,
            Monto = g.Monto
        }));

        // Gastos: semanales
        var semanales = await _context.GastosSemanales
            .AsNoTracking()
            .Where(g => g.ProyectoId == proyectoId)
            .ToListAsync();
        items.AddRange(semanales.Select(g => new HistorialFinancieroItemDto
        {
            Fecha = g.FechaInicio,
            Tipo = "Gasto",
            Categoria = "Gasto Semanal",
            Concepto = g.Concepto,
            Monto = g.Monto
        }));

        return items.OrderBy(i => i.Fecha).ToList();
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

    public async Task<(bool Success, string Message, object? InventarioAfectado)> DeleteAsync(int id, bool liberarInventario = false)
    {
        var proyecto = await _proyectosRepo.GetByIdAsync(id);
        if (proyecto is null) return (false, "Proyecto no encontrado.", null);

        var materialesConStock = await _context.AlmacenProyecto
            .Include(ap => ap.Material)
            .Where(ap => ap.ProyectoId == id && ap.Stock > 0)
            .ToListAsync();

        var herramientasActivas = await _context.AsignacionesHerramienta
            .Include(a => a.Herramienta)
            .Where(a => a.ProyectoId == id && a.Estado == EstadoAsignacion.Asignada)
            .ToListAsync();

        if ((materialesConStock.Any() || herramientasActivas.Any()) && !liberarInventario)
        {
            var inventario = new
            {
                materiales = materialesConStock.Select(m => new { m.Material!.Nombre, m.Stock }),
                herramientas = herramientasActivas.Select(h => new { h.Herramienta!.Nombre, h.Herramienta.Codigo })
            };
            return (false, "El proyecto tiene inventario asignado. Usa liberarInventario=true para devolverlo automáticamente al almacén.", inventario);
        }

        if (liberarInventario)
        {
            foreach (var ap in materialesConStock)
            {
                var central = await _context.AlmacenCentral
                    .FirstOrDefaultAsync(c => c.MaterialId == ap.MaterialId);
                if (central is not null)
                    central.Stock += ap.Stock;
                ap.Stock = 0;
            }

            foreach (var asig in herramientasActivas)
            {
                asig.Estado = EstadoAsignacion.Devuelta;
                asig.FechaDevolucion = DateTime.UtcNow;
                asig.ObservacionesDevolucion = "Devuelto automáticamente al eliminar proyecto.";
                var tieneOtrasAsignaciones = await _context.AsignacionesHerramienta
                    .AnyAsync(a => a.HerramientaId == asig.HerramientaId
                                && a.Id != asig.Id
                                && a.Estado == EstadoAsignacion.Asignada);
                if (!tieneOtrasAsignaciones && asig.Herramienta is not null)
                    asig.Herramienta.Estado = EstadoHerramienta.Disponible;
            }

            await _context.SaveChangesAsync();
        }

        await _proyectosRepo.DeleteAsync(proyecto);
        var (uid, uname) = GetUsuarioInfo();
        await _bitacora.RegistrarAsync(uid, uname, "Eliminó", "Proyecto",
            $"Proyecto '{proyecto.Nombre}' eliminado{(liberarInventario ? " con devolución de inventario" : "")}");
        return (true, "Proyecto eliminado correctamente.", null);
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

        return (true, "Planeación generada correctamente.", pdfBytes);
    }

    private (int UsuarioId, string NombreUsuario) GetUsuarioInfo()
    {
        var user = _httpContextAccessor.HttpContext?.User;
        var id = int.TryParse(user?.FindFirstValue(ClaimTypes.NameIdentifier), out var parsed) ? parsed : 0;
        var nombre = user?.FindFirstValue("nombreUsuario") ?? "Sistema";
        return (id, nombre);
    }
}

