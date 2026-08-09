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

    // ─── LIGA PÚBLICA (operador interno) ───────────────────────────────────
    Task<(bool Success, string Message, ServicioLigaDto? Data)> GenerarLigaAsync(int servicioId, int usuarioId);
    Task<ServicioLigaDto?> GetLigaActualAsync(int servicioId, int usuarioId);

    // ─── ACCESO POR TOKEN (sin sesión) ─────────────────────────────────────
    Task<(bool Valido, string Motivo, ServicioPublicoDto? Data)> GetPorTokenAsync(string token);
    Task<(bool Success, string Message, ServicioPublicoDto? Data)> ActualizarPorTokenAsync(string token, ServicioPublicoUpdateDto dto);
    Task<(bool Success, string Message, List<ServicioFotoDto> Data)> GetFotosPorTokenAsync(string token);
    Task<(bool Success, string Message, ServicioFotoDto? Data)> SubirFotoPorTokenAsync(string token, IFormFile foto);
    Task<(bool Found, string NombreOriginal, string ContentType, byte[]? Contenido)> DescargarFotoPorTokenAsync(string token, int fotoId);
    Task<(bool Success, string Message)> EliminarFotoPorTokenAsync(string token, int fotoId);
    Task<(bool Success, string Message, ServicioPublicoDto? Data)> FirmarPorTokenAsync(string token, ServicioFirmarDto dto);
}
