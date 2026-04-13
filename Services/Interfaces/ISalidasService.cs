using BLL_ConstruccionAPI.DTOs.Materiales;
using BLL_ConstruccionAPI.DTOs.Salidas;

namespace BLL_ConstruccionAPI.Services.Interfaces;

public interface ISalidasService
{
    Task<IEnumerable<SalidaResponseDto>> GetAllAsync();
    Task<IEnumerable<SalidaResponseDto>> GetByProyectoAsync(int proyectoId);
    Task<SalidaResponseDto?> GetByIdAsync(int id);
    Task<IEnumerable<AlmacenProyectoResponseDto>> GetAlmacenProyectoAsync(int proyectoId);
    Task<(bool Success, string Message, SalidaResponseDto? Data)> RegistrarAsync(SalidaRequestDto dto, int usuarioId);
    Task<(bool Success, string Message)> DevolverMaterialesAsync(int proyectoId, DevolucionRequestDto dto);
}
