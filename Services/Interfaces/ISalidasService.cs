using BLL_ConstruccionAPI.DTOs.Salidas;
using BLL_ConstruccionAPI.Models.Inventario.Materiales;

namespace BLL_ConstruccionAPI.Services.Interfaces;

public interface ISalidasService
{
    Task<IEnumerable<Salida>> GetAllAsync();
    Task<IEnumerable<Salida>> GetByProyectoAsync(int proyectoId);
    Task<Salida?> GetByIdAsync(int id);
    Task<IEnumerable<AlmacenProyecto>> GetAlmacenProyectoAsync(int proyectoId);
    Task<(bool Success, string Message, Salida? Data)> RegistrarAsync(SalidaRequestDto dto);
}
