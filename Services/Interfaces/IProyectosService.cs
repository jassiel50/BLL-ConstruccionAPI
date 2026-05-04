using BLL_ConstruccionAPI.DTOs.Herramientas;
using BLL_ConstruccionAPI.DTOs.Materiales;
using BLL_ConstruccionAPI.DTOs.Proyectos;

namespace BLL_ConstruccionAPI.Services.Interfaces;

public interface IProyectosService
{
    Task<IEnumerable<ProyectoResponseDto>> GetAllAsync();
    Task<IEnumerable<ProyectoResponseDto>> GetByClienteAsync(int clienteId);
    Task<ProyectoResponseDto?> GetByIdAsync(int id);
    Task<(bool Success, string Message, ProyectoResponseDto? Data)> CreateAsync(ProyectoRequestDto dto);
    Task<(bool Success, string Message)> UpdateAsync(int id, ProyectoRequestDto dto);
    Task<(bool Success, string Message)> DeleteAsync(int id);
    Task<(bool Success, string Message)> TerminarAsync(int id);
    Task<IEnumerable<AlmacenProyectoResponseDto>> GetMaterialesAsync(int proyectoId);
    Task<IEnumerable<AsignacionHerramientaResponseDto>> GetHerramientasAsync(int proyectoId);
    Task<(bool Success, string Message, int Count)> DevolverTodasHerramientasAsync(int proyectoId);
}
