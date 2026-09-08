using BLL_ConstruccionAPI.DTOs.Reportes;
using Microsoft.AspNetCore.Http;

namespace BLL_ConstruccionAPI.Services.Interfaces;

public interface IReporteEntregaConformidadService
{
    Task<List<ReporteEntregaConformidadDto>> GetAllAsync(int? proyectoId = null);
    Task<ReporteEntregaConformidadDto?> GetByIdAsync(int id);
    Task<(bool Success, string Message, ReporteEntregaConformidadDto? Data)> CreateAsync(ReporteEntregaConformidadRequestDto dto, int usuarioId);
    Task<(bool Success, string Message)> DeleteAsync(int id);

    Task<(bool Success, string Message)> AgregarFotosAsync(int id, List<IFormFile> fotos);
    Task<(bool Found, string ContentType, byte[]? Contenido)> DescargarFotoAsync(int fotoId);
    Task<(bool Success, string Message)> EliminarFotoAsync(int fotoId);

    Task<(bool Success, string Message)> SubirDocumentoFirmadoAsync(int id, IFormFile archivo);
    Task<(bool Found, string? NombreOriginal, string? ContentType, byte[]? Contenido)> DescargarDocumentoFirmadoAsync(int id);

    Task<(bool Success, string Message, byte[]? Pdf)> GenerarPdfAsync(int id);
}
