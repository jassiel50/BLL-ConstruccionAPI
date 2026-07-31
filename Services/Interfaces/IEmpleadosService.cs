using BLL_ConstruccionAPI.DTOs.Personal;
using Microsoft.AspNetCore.Http;

namespace BLL_ConstruccionAPI.Services.Interfaces;

public interface IEmpleadosService
{
    Task<List<EmpleadoResponseDto>> GetAllAsync();
    Task<EmpleadoResponseDto?> GetByIdAsync(int id);
    Task<(bool Success, string Message, EmpleadoResponseDto? Data)> CreateAsync(EmpleadoRequestDto dto);
    Task<(bool Success, string Message, EmpleadoResponseDto? Data)> UpdateAsync(int id, EmpleadoRequestDto dto);
    Task<(bool Success, string Message)> ToggleEstatusAsync(int id);

    Task<List<ArchivoEmpleadoDto>> GetArchivosAsync(int empleadoId);
    Task<(bool Success, string Message, ArchivoEmpleadoDto? Data)> SubirArchivoAsync(int empleadoId, IFormFile archivo, string tipoDocumento);
    Task<(bool Found, string NombreOriginal, string ContentType, byte[]? Contenido)> DescargarArchivoAsync(int id);
    Task<(bool Success, string Message)> EliminarArchivoAsync(int id);

    Task<List<AsignacionEmpleadoResponseDto>> GetAsignacionesAsync(int empleadoId);
    Task<(bool Success, string Message, AsignacionEmpleadoResponseDto? Data)> AsignarAsync(int empleadoId, AsignacionEmpleadoRequestDto dto, int usuarioId);
    Task<(bool Success, string Message)> FinalizarAsignacionAsync(int asignacionId, AsignacionEmpleadoFinalizarDto dto);

    Task<byte[]> GenerarContratoAsync(int empleadoId, GenerarContratoRequestDto dto);
}
