using BLL_ConstruccionAPI.DTOs.Herramientas;
using BLL_ConstruccionAPI.Models.Enums;
using BLL_ConstruccionAPI.Models.Inventario.Herramientas;
using BLL_ConstruccionAPI.Repositories.Interfaces;
using BLL_ConstruccionAPI.Services.Interfaces;

namespace BLL_ConstruccionAPI.Services;

public class HerramientasService : IHerramientasService
{
    private readonly IHerramientasRepository _herramientasRepo;
    private readonly ICatalogosRepository _catalogosRepo;
    private readonly IProyectosRepository _proyectosRepo;

    private static readonly EstadoHerramienta[] EstadosValidos =
        [EstadoHerramienta.Disponible, EstadoHerramienta.Asignada, EstadoHerramienta.Mantenimiento, EstadoHerramienta.Baja];

    public HerramientasService(
        IHerramientasRepository herramientasRepo,
        ICatalogosRepository catalogosRepo,
        IProyectosRepository proyectosRepo)
    {
        _herramientasRepo = herramientasRepo;
        _catalogosRepo = catalogosRepo;
        _proyectosRepo = proyectosRepo;
    }

    public async Task<IEnumerable<HerramientaResponseDto>> GetAllAsync()
    {
        var herramientas = await _herramientasRepo.GetAllAsync();
        return herramientas.Select(HerramientaResponseDto.FromEntity);
    }

    public async Task<IEnumerable<HerramientaResponseDto>> GetDisponiblesAsync()
    {
        var herramientas = await _herramientasRepo.GetDisponiblesAsync();
        return herramientas.Select(HerramientaResponseDto.FromEntity);
    }

    public async Task<HerramientaResponseDto?> GetByIdAsync(int id)
    {
        var herramienta = await _herramientasRepo.GetByIdAsync(id);
        return herramienta is null ? null : HerramientaResponseDto.FromEntity(herramienta);
    }

    public async Task<IEnumerable<AsignacionHerramientaResponseDto>> GetAsignacionesAsync(int herramientaId)
    {
        var asignaciones = await _herramientasRepo.GetAsignacionesByHerramientaAsync(herramientaId);
        return asignaciones.Select(AsignacionHerramientaResponseDto.FromEntity);
    }

    public async Task<(bool Success, string Message, HerramientaResponseDto? Data)> CreateAsync(HerramientaRequestDto dto)
    {
        if (!Enum.TryParse<EstadoHerramienta>(dto.Estado, out var estadoHerramienta))
            return (false, $"Estado inválido. Valores permitidos: {string.Join(", ", Enum.GetNames<EstadoHerramienta>())}.", null);

        if (!Enum.TryParse<Zona>(dto.Zona, out var zona))
            return (false, $"Zona inválida. Valores permitidos: {string.Join(", ", Enum.GetNames<Zona>())}.", null);

        if (!Enum.TryParse<TipoUbicacion>(dto.TipoUbicacion, out var tipoUbicacion))
            return (false, $"TipoUbicacion inválido. Valores permitidos: {string.Join(", ", Enum.GetNames<TipoUbicacion>())}.", null);

        if (await _herramientasRepo.ExisteCodigoAsync(dto.Codigo))
            return (false, "Ya existe una herramienta con ese código.", null);

        if (await _herramientasRepo.ExisteNumeroSerieAsync(dto.NumeroSerie))
            return (false, "Ya existe una herramienta con ese número de serie.", null);

        var categoria = await _catalogosRepo.GetCategoriaHerramientaByIdAsync(dto.CategoriaHerramientaId);
        if (categoria is null || !categoria.Activo)
            return (false, "La categoría de herramienta especificada no existe o está inactiva.", null);

        var herramienta = new Herramienta
        {
            Nombre = dto.Nombre,
            Descripcion = dto.Descripcion,
            Codigo = dto.Codigo,
            NumeroSerie = dto.NumeroSerie,
            CategoriaHerramientaId = dto.CategoriaHerramientaId,
            Estado = estadoHerramienta,
            Zona = zona,
            TipoUbicacion = tipoUbicacion,
            ValorAdquisicion = dto.ValorAdquisicion,
            FechaAdquisicion = dto.FechaAdquisicion,
            Cantidad = dto.Cantidad,
            Activo = true,
            FechaRegistro = DateTime.UtcNow
        };

        await _herramientasRepo.CreateAsync(herramienta);
        return (true, "Herramienta registrada correctamente.", HerramientaResponseDto.FromEntity(herramienta));
    }

    public async Task<(bool Success, string Message)> UpdateAsync(int id, HerramientaRequestDto dto)
    {
        var herramienta = await _herramientasRepo.GetByIdAsync(id);
        if (herramienta is null) return (false, "Herramienta no encontrada.");

        if (!Enum.TryParse<EstadoHerramienta>(dto.Estado, out var estadoActualizado))
            return (false, $"Estado inválido. Valores permitidos: {string.Join(", ", Enum.GetNames<EstadoHerramienta>())}.");

        if (!Enum.TryParse<Zona>(dto.Zona, out var zonaActualizada))
            return (false, $"Zona inválida. Valores permitidos: {string.Join(", ", Enum.GetNames<Zona>())}.");

        if (!Enum.TryParse<TipoUbicacion>(dto.TipoUbicacion, out var tipoUbicacionActualizado))
            return (false, $"TipoUbicacion inválido. Valores permitidos: {string.Join(", ", Enum.GetNames<TipoUbicacion>())}.");

        if (herramienta.Codigo != dto.Codigo && await _herramientasRepo.ExisteCodigoAsync(dto.Codigo))
            return (false, "Ya existe una herramienta con ese código.");

        if (herramienta.NumeroSerie != dto.NumeroSerie && await _herramientasRepo.ExisteNumeroSerieAsync(dto.NumeroSerie))
            return (false, "Ya existe una herramienta con ese número de serie.");

        var categoria = await _catalogosRepo.GetCategoriaHerramientaByIdAsync(dto.CategoriaHerramientaId);
        if (categoria is null || !categoria.Activo)
            return (false, "La categoría de herramienta especificada no existe o está inactiva.");

        herramienta.Nombre = dto.Nombre;
        herramienta.Descripcion = dto.Descripcion;
        herramienta.Codigo = dto.Codigo;
        herramienta.NumeroSerie = dto.NumeroSerie;
        herramienta.CategoriaHerramientaId = dto.CategoriaHerramientaId;
        herramienta.Estado = estadoActualizado;
        herramienta.Zona = zonaActualizada;
        herramienta.TipoUbicacion = tipoUbicacionActualizado;
        herramienta.ValorAdquisicion = dto.ValorAdquisicion;
        herramienta.FechaAdquisicion = dto.FechaAdquisicion;
        herramienta.Cantidad = dto.Cantidad;

        await _herramientasRepo.UpdateAsync(herramienta);
        return (true, "Herramienta actualizada correctamente.");
    }

    public async Task<(bool Success, string Message)> DeleteAsync(int id)
    {
        var herramienta = await _herramientasRepo.GetByIdAsync(id);
        if (herramienta is null) return (false, "Herramienta no encontrada.");

        if (herramienta.Estado == EstadoHerramienta.Asignada)
            return (false, "No se puede dar de baja una herramienta que está asignada a un proyecto.");

        await _herramientasRepo.DeleteAsync(herramienta);
        return (true, "Herramienta dada de baja correctamente.");
    }

    public async Task<(bool Success, string Message, AsignacionHerramientaResponseDto? Data)> AsignarAsync(AsignacionRequestDto dto, int usuarioId)
    {
        var herramienta = await _herramientasRepo.GetByIdAsync(dto.HerramientaId);
        if (herramienta is null || !herramienta.Activo)
            return (false, "Herramienta no encontrada.", null);

        if (herramienta.Estado != EstadoHerramienta.Disponible)
            return (false, $"La herramienta no está disponible. Estado actual: {herramienta.Estado}.", null);

        var proyecto = await _proyectosRepo.GetByIdAsync(dto.ProyectoId);
        if (proyecto is null)
            return (false, "El proyecto especificado no existe.", null);

        var asignacion = new AsignacionHerramienta
        {
            HerramientaId = dto.HerramientaId,
            ProyectoId = dto.ProyectoId,
            UsuarioAsignoId = usuarioId,
            UsuarioRecibeId = dto.UsuarioRecibeId,
            FechaAsignacion = DateTime.UtcNow,
            Estado = EstadoAsignacion.Asignada,
            Observaciones = dto.Observaciones
        };

        herramienta.Estado = EstadoHerramienta.Asignada;
        await _herramientasRepo.AsignarHerramientaAsync(asignacion, herramienta);

        return (true, "Herramienta asignada correctamente.", AsignacionHerramientaResponseDto.FromEntity(asignacion));
    }

    public async Task<(bool Success, string Message)> DevolverAsync(int asignacionId, DevolucionRequestDto dto)
    {
        var asignacion = await _herramientasRepo.GetAsignacionByIdAsync(asignacionId);
        if (asignacion is null) return (false, "Asignación no encontrada.");

        if (asignacion.Estado != EstadoAsignacion.Asignada)
            return (false, "Esta asignación ya fue devuelta.");

        asignacion.Estado = EstadoAsignacion.Devuelta;
        asignacion.FechaDevolucion = DateTime.UtcNow;
        asignacion.ObservacionesDevolucion = dto.ObservacionesDevolucion;

        var herramienta = await _herramientasRepo.GetByIdAsync(asignacion.HerramientaId);
        if (herramienta is null)
            return (false, "La herramienta asociada a la asignación no fue encontrada.");

        herramienta.Estado = EstadoHerramienta.Disponible;

        await _herramientasRepo.DevolverHerramientaAsync(asignacion, herramienta);

        return (true, "Herramienta devuelta correctamente.");
    }
}
