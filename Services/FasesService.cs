using BLL_ConstruccionAPI.DTOs.Fases;
using BLL_ConstruccionAPI.Models.Enums;
using BLL_ConstruccionAPI.Models.Inventario.Proyectos;
using BLL_ConstruccionAPI.Repositories.Interfaces;
using BLL_ConstruccionAPI.Services.Interfaces;

namespace BLL_ConstruccionAPI.Services;

public class FasesService : IFasesService
{
    private readonly IFasesRepository _fasesRepo;
    private readonly IProyectosRepository _proyectosRepo;

    public FasesService(IFasesRepository fasesRepo, IProyectosRepository proyectosRepo)
    {
        _fasesRepo = fasesRepo;
        _proyectosRepo = proyectosRepo;
    }

    public async Task<List<FaseResponseDto>> GetByProyectoAsync(int proyectoId)
    {
        var fases = await _fasesRepo.GetByProyectoAsync(proyectoId);
        return fases.Select(FaseResponseDto.FromEntity).ToList();
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
        return (true, "Fase marcada como completada.");
    }

    public async Task<(bool Success, string Message)> DeleteAsync(int id)
    {
        var fase = await _fasesRepo.GetByIdAsync(id);
        if (fase is null) return (false, "Fase no encontrada.");

        if (fase.Estado == EstadoFase.Completada)
            return (false, "No se puede eliminar una fase ya completada.");

        await _fasesRepo.DeleteAsync(fase);
        return (true, "Fase eliminada correctamente.");
    }

    public async Task<List<FaseResponseDto>> GetAtrasadasAsync()
    {
        var fases = await _fasesRepo.GetAtrasadasAsync();
        return fases.Select(FaseResponseDto.FromEntity).ToList();
    }
}
