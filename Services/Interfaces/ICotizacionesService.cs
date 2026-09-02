using BLL_ConstruccionAPI.DTOs.Cotizaciones;

namespace BLL_ConstruccionAPI.Services.Interfaces;

public interface ICotizacionesService
{
    Task<List<CotizacionResponseDto>> GetAllAsync();
    Task<CotizacionDetalleDto?> GetByIdAsync(int id);
    Task<(bool Success, string Message, CotizacionResponseDto? Data)> GenerarAsync(CotizacionRequestDto dto, int usuarioId);
    Task<(bool Success, string Message, CotizacionResponseDto? Data)> ActualizarAsync(int id, CotizacionRequestDto dto);
    Task<(bool Found, byte[]? Contenido, string Folio)> DescargarAsync(int id);

    Task<(bool Success, string Message, CotizacionResponseDto? Data)> GuardarBorradorNuevoAsync(CotizacionRequestDto dto, int usuarioId);
    Task<(bool Success, string Message)> GuardarBorradorExistenteAsync(int id, CotizacionRequestDto dto);
    Task<(bool Success, string Message)> EliminarAsync(int id);
}
