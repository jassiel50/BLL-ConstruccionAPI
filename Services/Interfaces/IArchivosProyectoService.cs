using BLL_ConstruccionAPI.DTOs.Archivos;
using Microsoft.AspNetCore.Http;

namespace BLL_ConstruccionAPI.Services.Interfaces;

public interface IArchivosProyectoService
{
    Task<List<ArchivoProyectoDto>> GetByProyectoAsync(int proyectoId);
    Task<(bool Success, string Message, ArchivoProyectoDto? Data)> SubirAsync(int proyectoId, IFormFile archivo, string tipoDocumento, int? carpetaId);
    Task<(bool Found, string NombreOriginal, string ContentType, byte[]? Contenido)> DescargarAsync(int id);
    Task<(bool Success, string Message)> DeleteAsync(int id);

    Task<List<CarpetaProyectoDto>> GetCarpetasAsync(int proyectoId);
    Task<(bool Success, string Message, CarpetaProyectoDto? Data)> CrearCarpetaAsync(int proyectoId, CarpetaProyectoRequestDto dto);
    Task<(bool Success, string Message)> EliminarCarpetaAsync(int carpetaId);
}
