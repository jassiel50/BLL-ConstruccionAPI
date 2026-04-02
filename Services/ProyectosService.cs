using BLL_ConstruccionAPI.DTOs.Proyectos;
using BLL_ConstruccionAPI.Models.Inventario.Proyectos;
using BLL_ConstruccionAPI.Repositories.Interfaces;
using BLL_ConstruccionAPI.Services.Interfaces;

namespace BLL_ConstruccionAPI.Services;

public class ProyectosService : IProyectosService
{
    private readonly IProyectosRepository _proyectosRepo;
    private readonly IProveedoresClientesRepository _provClientesRepo;

    private static readonly string[] EstadosValidos = ["Activo", "Pausado", "Terminado"];

    public ProyectosService(IProyectosRepository proyectosRepo, IProveedoresClientesRepository provClientesRepo)
    {
        _proyectosRepo = proyectosRepo;
        _provClientesRepo = provClientesRepo;
    }

    public async Task<IEnumerable<Proyecto>> GetAllAsync()
        => await _proyectosRepo.GetAllAsync();

    public async Task<IEnumerable<Proyecto>> GetByClienteAsync(int clienteId)
        => await _proyectosRepo.GetByClienteAsync(clienteId);

    public async Task<Proyecto?> GetByIdAsync(int id)
        => await _proyectosRepo.GetByIdAsync(id);

    public async Task<(bool Success, string Message, Proyecto? Data)> CreateAsync(ProyectoRequestDto dto)
    {
        if (!EstadosValidos.Contains(dto.Estado))
            return (false, $"Estado inválido. Los valores permitidos son: {string.Join(", ", EstadosValidos)}.", null);

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
            Estado = dto.Estado,
            FechaRegistro = DateTime.UtcNow
        };

        await _proyectosRepo.CreateAsync(proyecto);
        return (true, "Proyecto creado correctamente.", proyecto);
    }

    public async Task<(bool Success, string Message)> UpdateAsync(int id, ProyectoRequestDto dto)
    {
        var proyecto = await _proyectosRepo.GetByIdAsync(id);
        if (proyecto is null) return (false, "Proyecto no encontrado.");

        if (!EstadosValidos.Contains(dto.Estado))
            return (false, $"Estado inválido. Los valores permitidos son: {string.Join(", ", EstadosValidos)}.");

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
        proyecto.Estado = dto.Estado;

        await _proyectosRepo.UpdateAsync(proyecto);
        return (true, "Proyecto actualizado correctamente.");
    }

    public async Task<(bool Success, string Message)> DeleteAsync(int id)
    {
        var proyecto = await _proyectosRepo.GetByIdAsync(id);
        if (proyecto is null) return (false, "Proyecto no encontrado.");

        await _proyectosRepo.DeleteAsync(proyecto);
        return (true, "Proyecto marcado como Terminado correctamente.");
    }
}
