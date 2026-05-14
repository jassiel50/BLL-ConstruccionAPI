using BLL_ConstruccionAPI.DTOs.Archivos;
using Microsoft.AspNetCore.Http;

namespace BLL_ConstruccionAPI.Services.Interfaces;

public interface IArchivosProyectoService
{
    Task<List<ArchivoProyectoDto>> GetByProyectoAsync(int proyectoId);
    Task<(bool Success, string Message, ArchivoProyectoDto? Data)> SubirAsync(int proyectoId, IFormFile archivo, string tipoDocumento);
    Task<(bool Found, string NombreOriginal, string ContentType, byte[]? Contenido)> DescargarAsync(int id);
    Task<(bool Success, string Message)> DeleteAsync(int id);
}
