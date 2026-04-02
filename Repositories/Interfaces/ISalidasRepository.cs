using BLL_ConstruccionAPI.Models.Inventario.Materiales;

namespace BLL_ConstruccionAPI.Repositories.Interfaces;

public interface ISalidasRepository
{
    Task<IEnumerable<Salida>> GetAllAsync();
    Task<IEnumerable<Salida>> GetByProyectoAsync(int proyectoId);
    Task<Salida?> GetByIdAsync(int id);
    Task<bool> ExisteFolioAsync(string numeroFolio);

    // AlmacenProyecto
    Task<AlmacenProyecto?> GetStockProyectoAsync(int proyectoId, int materialId);
    Task<IEnumerable<AlmacenProyecto>> GetAlmacenProyectoAsync(int proyectoId);

    // Agrega AlmacenProyecto al contexto sin guardar (se incluye en el SaveChangesAsync final)
    void TrackNuevoStockProyecto(AlmacenProyecto almacen);

    // Guarda la Salida + Detalles + AlmacenCentral + AlmacenProyecto en un único SaveChangesAsync
    Task<int> RegistrarSalidaAsync(Salida salida);
}
