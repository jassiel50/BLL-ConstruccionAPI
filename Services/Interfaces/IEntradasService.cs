using BLL_ConstruccionAPI.DTOs.Entradas;

namespace BLL_ConstruccionAPI.Services.Interfaces;

public interface IEntradasService
{
    Task<IEnumerable<EntradaResponseDto>> GetAllAsync();
    Task<EntradaResponseDto?> GetByIdAsync(int id);
    Task<(bool Success, string Message, EntradaResponseDto? Data)> RegistrarAsync(EntradaRequestDto dto, int usuarioId);
}
