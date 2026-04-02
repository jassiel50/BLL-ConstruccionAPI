using BLL_ConstruccionAPI.DTOs.Herramientas;
using BLL_ConstruccionAPI.Models.Inventario.Herramientas;

namespace BLL_ConstruccionAPI.Services.Interfaces;

public interface IHerramientasService
{
    Task<IEnumerable<Herramienta>> GetAllAsync();
    Task<IEnumerable<Herramienta>> GetDisponiblesAsync();
    Task<Herramienta?> GetByIdAsync(int id);
    Task<IEnumerable<AsignacionHerramienta>> GetAsignacionesAsync(int herramientaId);
    Task<(bool Success, string Message, Herramienta? Data)> CreateAsync(HerramientaRequestDto dto);
    Task<(bool Success, string Message)> UpdateAsync(int id, HerramientaRequestDto dto);
    Task<(bool Success, string Message)> DeleteAsync(int id);
    Task<(bool Success, string Message, AsignacionHerramienta? Data)> AsignarAsync(AsignacionRequestDto dto);
    Task<(bool Success, string Message)> DevolverAsync(int asignacionId, DevolucionRequestDto dto);
}
