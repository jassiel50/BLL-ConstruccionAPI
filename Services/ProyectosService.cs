using BLL_ConstruccionAPI.DTOs.Herramientas;
using BLL_ConstruccionAPI.DTOs.Materiales;
using BLL_ConstruccionAPI.DTOs.Proyectos;
using BLL_ConstruccionAPI.Models.Enums;
using BLL_ConstruccionAPI.Models.Inventario.Proyectos;
using BLL_ConstruccionAPI.Repositories.Interfaces;
using BLL_ConstruccionAPI.Services.Interfaces;

namespace BLL_ConstruccionAPI.Services;

public class ProyectosService : IProyectosService
{
    private readonly IProyectosRepository _proyectosRepo;
    private readonly IProveedoresClientesRepository _provClientesRepo;

    private static readonly EstadoProyecto[] EstadosValidos =
        [EstadoProyecto.Activo, EstadoProyecto.Pausado, EstadoProyecto.Terminado];

    public ProyectosService(IProyectosRepository proyectosRepo, IProveedoresClientesRepository provClientesRepo)
    {
        _proyectosRepo = proyectosRepo;
        _provClientesRepo = provClientesRepo;
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
        return proyecto is null ? null : ProyectoResponseDto.FromEntity(proyecto);
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
            Cliente = cliente
        };

        await _proyectosRepo.CreateAsync(proyecto);
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

        await _proyectosRepo.UpdateAsync(proyecto);
        return (true, "Proyecto actualizado correctamente.");
    }

    public async Task<(bool Success, string Message)> DeleteAsync(int id)
    {
        var proyecto = await _proyectosRepo.GetByIdAsync(id);
        if (proyecto is null) return (false, "Proyecto no encontrado.");

        await _proyectosRepo.DeleteAsync(proyecto);
        return (true, "Proyecto eliminado correctamente.");
    }

    public async Task<(bool Success, string Message)> TerminarAsync(int id)
    {
        var proyecto = await _proyectosRepo.GetByIdAsync(id);
        if (proyecto is null) return (false, "Proyecto no encontrado.");

        if (proyecto.Estado == EstadoProyecto.Terminado)
            return (false, "El proyecto ya está marcado como Terminado.");

        await _proyectosRepo.TerminarAsync(proyecto);
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
}
