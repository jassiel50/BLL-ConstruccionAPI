using BLL_ConstruccionAPI.DTOs.Entradas;
using BLL_ConstruccionAPI.Models.Inventario.Materiales;

namespace BLL_ConstruccionAPI.Services.Interfaces;

public interface IEntradasService
{
    Task<IEnumerable<Entrada>> GetAllAsync();
    Task<Entrada?> GetByIdAsync(int id);
    Task<(bool Success, string Message, Entrada? Data)> RegistrarAsync(EntradaRequestDto dto);
}
