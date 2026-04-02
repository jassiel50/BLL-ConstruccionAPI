using BLL_ConstruccionAPI.DTOs.Proyectos;
using BLL_ConstruccionAPI.Models.Inventario.Proyectos;

namespace BLL_ConstruccionAPI.Services.Interfaces;

public interface IProyectosService
{
    Task<IEnumerable<Proyecto>> GetAllAsync();
    Task<IEnumerable<Proyecto>> GetByClienteAsync(int clienteId);
    Task<Proyecto?> GetByIdAsync(int id);
    Task<(bool Success, string Message, Proyecto? Data)> CreateAsync(ProyectoRequestDto dto);
    Task<(bool Success, string Message)> UpdateAsync(int id, ProyectoRequestDto dto);
    Task<(bool Success, string Message)> DeleteAsync(int id);
}
