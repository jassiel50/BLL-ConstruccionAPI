using BLL_ConstruccionAPI.DTOs.Servicios;
using Microsoft.AspNetCore.Http;

namespace BLL_ConstruccionAPI.Services.Interfaces;

public interface IServiciosService
{
    Task<IEnumerable<ServicioResponseDto>> GetAllAsync(int solicitanteRolId, int solicitanteUsuarioId);
    Task<ServicioResponseDto?> GetByIdAsync(int id, int solicitanteRolId, int solicitanteUsuarioId);
    Task<(bool Success, string Message, ServicioResponseDto? Data)> CreateAsync(int usuarioId, string nombreUsuario, ServicioRequestDto dto);
    Task<(bool Success, string Message, ServicioResponseDto? Data)> UpdateAsync(int id, int usuarioId, ServicioRequestDto dto);
    Task<(bool Success, string Message, ServicioResponseDto? Data)> FirmarAsync(int id, int usuarioId, ServicioFirmarDto dto);
    Task<(bool Success, string Message)> DeleteAsync(int id, int usuarioId);

    Task<(bool Success, string Message, List<ServicioFotoDto> Data)> GetFotosAsync(int servicioId, int solicitanteRolId, int solicitanteUsuarioId);
    Task<(bool Success, string Message, ServicioFotoDto? Data)> SubirFotoAsync(int servicioId, int usuarioId, IFormFile foto);
    Task<(bool Found, string NombreOriginal, string ContentType, byte[]? Contenido)> DescargarFotoAsync(int fotoId, int solicitanteRolId, int solicitanteUsuarioId);
    Task<(bool Success, string Message)> EliminarFotoAsync(int fotoId, int usuarioId);

    Task<byte[]> GenerarReporteAsync(int servicioId, int solicitanteRolId, int solicitanteUsuarioId);
}
