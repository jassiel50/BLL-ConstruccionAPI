using BLL_ConstruccionAPI.DTOs.Servicios;

namespace BLL_ConstruccionAPI.Services.Interfaces;

public interface IServiciosService
{
    Task<IEnumerable<ServicioResponseDto>> GetAllAsync(int solicitanteRolId, int solicitanteUsuarioId);
    Task<ServicioResponseDto?> GetByIdAsync(int id, int solicitanteRolId, int solicitanteUsuarioId);
    Task<(bool Success, string Message, ServicioResponseDto? Data)> CreateAsync(int usuarioId, string nombreUsuario, ServicioRequestDto dto);
    Task<(bool Success, string Message, ServicioResponseDto? Data)> UpdateAsync(int id, int usuarioId, ServicioRequestDto dto);
    Task<(bool Success, string Message, ServicioResponseDto? Data)> FirmarAsync(int id, int usuarioId, ServicioFirmarDto dto);
    Task<(bool Success, string Message)> DeleteAsync(int id, int usuarioId);
}
