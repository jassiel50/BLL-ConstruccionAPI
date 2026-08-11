using BLL_ConstruccionAPI.DTOs.Cotizaciones;

namespace BLL_ConstruccionAPI.Services.Interfaces;

public interface ICotizacionesService
{
    Task<List<CotizacionResponseDto>> GetAllAsync();
    Task<(bool Success, string Message, CotizacionResponseDto? Data)> GenerarAsync(CotizacionRequestDto dto, int usuarioId);
    Task<(bool Found, byte[]? Contenido, string Folio)> DescargarAsync(int id);
}
