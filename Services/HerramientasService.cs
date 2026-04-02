using BLL_ConstruccionAPI.DTOs.Herramientas;
using BLL_ConstruccionAPI.Models.Inventario.Herramientas;
using BLL_ConstruccionAPI.Repositories.Interfaces;
using BLL_ConstruccionAPI.Services.Interfaces;

namespace BLL_ConstruccionAPI.Services;

public class HerramientasService : IHerramientasService
{
    private readonly IHerramientasRepository _herramientasRepo;
    private readonly ICatalogosRepository _catalogosRepo;
    private readonly IProyectosRepository _proyectosRepo;

    private static readonly string[] EstadosValidos = ["Disponible", "Asignada", "Mantenimiento", "Baja"];

    public HerramientasService(
        IHerramientasRepository herramientasRepo,
        ICatalogosRepository catalogosRepo,
        IProyectosRepository proyectosRepo)
    {
        _herramientasRepo = herramientasRepo;
        _catalogosRepo = catalogosRepo;
        _proyectosRepo = proyectosRepo;
    }

    public async Task<IEnumerable<Herramienta>> GetAllAsync()
        => await _herramientasRepo.GetAllAsync();

    public async Task<IEnumerable<Herramienta>> GetDisponiblesAsync()
        => await _herramientasRepo.GetDisponiblesAsync();

    public async Task<Herramienta?> GetByIdAsync(int id)
        => await _herramientasRepo.GetByIdAsync(id);

    public async Task<IEnumerable<AsignacionHerramienta>> GetAsignacionesAsync(int herramientaId)
        => await _herramientasRepo.GetAsignacionesByHerramientaAsync(herramientaId);

    public async Task<(bool Success, string Message, Herramienta? Data)> CreateAsync(HerramientaRequestDto dto)
    {
        if (!EstadosValidos.Contains(dto.Estado))
            return (false, $"Estado inválido. Valores permitidos: {string.Join(", ", EstadosValidos)}.", null);

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
            Estado = dto.Estado,
            ValorAdquisicion = dto.ValorAdquisicion,
            FechaAdquisicion = dto.FechaAdquisicion,
            Activo = true,
            FechaRegistro = DateTime.UtcNow
        };

        await _herramientasRepo.CreateAsync(herramienta);
        return (true, "Herramienta registrada correctamente.", herramienta);
    }

    public async Task<(bool Success, string Message)> UpdateAsync(int id, HerramientaRequestDto dto)
    {
        var herramienta = await _herramientasRepo.GetByIdAsync(id);
        if (herramienta is null) return (false, "Herramienta no encontrada.");

        if (!EstadosValidos.Contains(dto.Estado))
            return (false, $"Estado inválido. Valores permitidos: {string.Join(", ", EstadosValidos)}.");

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
        herramienta.Estado = dto.Estado;
        herramienta.ValorAdquisicion = dto.ValorAdquisicion;
        herramienta.FechaAdquisicion = dto.FechaAdquisicion;

        await _herramientasRepo.UpdateAsync(herramienta);
        return (true, "Herramienta actualizada correctamente.");
    }

    public async Task<(bool Success, string Message)> DeleteAsync(int id)
    {
        var herramienta = await _herramientasRepo.GetByIdAsync(id);
        if (herramienta is null) return (false, "Herramienta no encontrada.");

        if (herramienta.Estado == "Asignada")
            return (false, "No se puede dar de baja una herramienta que está asignada a un proyecto.");

        await _herramientasRepo.DeleteAsync(herramienta);
        return (true, "Herramienta dada de baja correctamente.");
    }

    public async Task<(bool Success, string Message, AsignacionHerramienta? Data)> AsignarAsync(AsignacionRequestDto dto)
    {
        var herramienta = await _herramientasRepo.GetByIdAsync(dto.HerramientaId);
        if (herramienta is null || !herramienta.Activo)
            return (false, "Herramienta no encontrada.", null);

        if (herramienta.Estado != "Disponible")
            return (false, $"La herramienta no está disponible. Estado actual: {herramienta.Estado}.", null);

        var proyecto = await _proyectosRepo.GetByIdAsync(dto.ProyectoId);
        if (proyecto is null)
            return (false, "El proyecto especificado no existe.", null);

        var asignacion = new AsignacionHerramienta
        {
            HerramientaId = dto.HerramientaId,
            ProyectoId = dto.ProyectoId,
            UsuarioAsignoId = dto.UsuarioAsignoId,
            UsuarioRecibeId = dto.UsuarioRecibeId,
            FechaAsignacion = DateTime.UtcNow,
            Estado = "Asignada",
            Observaciones = dto.Observaciones
        };

        await _herramientasRepo.CreateAsignacionAsync(asignacion);

        herramienta.Estado = "Asignada";
        await _herramientasRepo.UpdateAsync(herramienta);

        return (true, "Herramienta asignada correctamente.", asignacion);
    }

    public async Task<(bool Success, string Message)> DevolverAsync(int asignacionId, DevolucionRequestDto dto)
    {
        var asignacion = await _herramientasRepo.GetAsignacionByIdAsync(asignacionId);
        if (asignacion is null) return (false, "Asignación no encontrada.");

        if (asignacion.Estado != "Asignada")
            return (false, "Esta asignación ya fue devuelta.");

        asignacion.Estado = "Devuelta";
        asignacion.FechaDevolucion = DateTime.UtcNow;
        asignacion.ObservacionesDevolucion = dto.ObservacionesDevolucion;
        await _herramientasRepo.UpdateAsignacionAsync(asignacion);

        var herramienta = await _herramientasRepo.GetByIdAsync(asignacion.HerramientaId);
        if (herramienta is not null)
        {
            herramienta.Estado = "Disponible";
            await _herramientasRepo.UpdateAsync(herramienta);
        }

        return (true, "Herramienta devuelta correctamente.");
    }
}
