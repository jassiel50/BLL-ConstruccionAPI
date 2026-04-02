using BLL_ConstruccionAPI.Models.Inventario.Materiales;

namespace BLL_ConstruccionAPI.Repositories.Interfaces;

public interface IEntradasRepository
{
    Task<IEnumerable<Entrada>> GetAllAsync();
    Task<Entrada?> GetByIdAsync(int id);
    Task<bool> ExisteFolioAsync(string numeroFolio);

    // Guarda la Entrada + Detalles + todos los cambios tracked (AlmacenCentral)
    // en un único SaveChangesAsync → operación atómica
    Task<int> RegistrarEntradaAsync(Entrada entrada);
}
