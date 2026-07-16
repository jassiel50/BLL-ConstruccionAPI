using BLL_ConstruccionAPI.DTOs.Herramientas;

namespace BLL_ConstruccionAPI.Services.Interfaces;

public interface IHerramientasService
{
    Task<IEnumerable<HerramientaResponseDto>> GetAllAsync();
    Task<IEnumerable<HerramientaResponseDto>> GetDisponiblesAsync();
    Task<HerramientaResponseDto?> GetByIdAsync(int id);
    Task<IEnumerable<AsignacionHerramientaResponseDto>> GetAsignacionesAsync(int herramientaId);
    Task<(bool Success, string Message, HerramientaResponseDto? Data)> CreateAsync(HerramientaRequestDto dto, bool registrarBitacora = true);
    Task<List<HerramientaBulkResultDto>> CreateBulkAsync(List<HerramientaRequestDto> dtos);
    Task<(bool Success, string Message)> UpdateAsync(int id, HerramientaRequestDto dto);
    Task<(bool Success, string Message)> DeleteAsync(int id);
    Task<(bool Success, string Message, AsignacionHerramientaResponseDto? Data)> AsignarAsync(AsignacionRequestDto dto, int usuarioId);
    Task<(bool Success, string Message)> DevolverAsync(int asignacionId, DevolucionRequestDto dto);

    Task<IEnumerable<AsignacionActivaDto>> GetAsignacionesActivasAsync();
    Task<List<AsignacionOperacionResultDto>> DevolverMultipleAsync(List<int> asignacionIds, string observacionesDevolucion);
    Task<(bool Success, string Message)> TransferirAsync(int asignacionId, int nuevoProyectoId, int usuarioId);
    Task<List<AsignacionOperacionResultDto>> TransferirMultipleAsync(List<int> asignacionIds, int nuevoProyectoId, int usuarioId);
    Task<(bool Success, string Message)> CambiarUbicacionAsync(int herramientaId, string tipoUbicacion);
}
