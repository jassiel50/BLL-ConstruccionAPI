using BLL_ConstruccionAPI.DTOs.Salidas;
using BLL_ConstruccionAPI.Models.Inventario.Materiales;
using BLL_ConstruccionAPI.Repositories.Interfaces;
using BLL_ConstruccionAPI.Services.Interfaces;

namespace BLL_ConstruccionAPI.Services;

public class SalidasService : ISalidasService
{
    private readonly ISalidasRepository _salidasRepo;
    private readonly IMaterialesRepository _materialesRepo;
    private readonly IProyectosRepository _proyectosRepo;

    public SalidasService(
        ISalidasRepository salidasRepo,
        IMaterialesRepository materialesRepo,
        IProyectosRepository proyectosRepo)
    {
        _salidasRepo = salidasRepo;
        _materialesRepo = materialesRepo;
        _proyectosRepo = proyectosRepo;
    }

    public async Task<IEnumerable<Salida>> GetAllAsync()
        => await _salidasRepo.GetAllAsync();

    public async Task<IEnumerable<Salida>> GetByProyectoAsync(int proyectoId)
        => await _salidasRepo.GetByProyectoAsync(proyectoId);

    public async Task<Salida?> GetByIdAsync(int id)
        => await _salidasRepo.GetByIdAsync(id);

    public async Task<IEnumerable<AlmacenProyecto>> GetAlmacenProyectoAsync(int proyectoId)
        => await _salidasRepo.GetAlmacenProyectoAsync(proyectoId);

    public async Task<(bool Success, string Message, Salida? Data)> RegistrarAsync(SalidaRequestDto dto)
    {
        if (!dto.Detalles.Any())
            return (false, "La salida debe tener al menos un detalle.", null);

        if (await _salidasRepo.ExisteFolioAsync(dto.NumeroFolio))
            return (false, $"Ya existe una salida con el folio '{dto.NumeroFolio}'.", null);

        var proyecto = await _proyectosRepo.GetByIdAsync(dto.ProyectoId);
        if (proyecto is null)
            return (false, "El proyecto especificado no existe.", null);

        var detalles = new List<SalidaDetalle>();

        foreach (var item in dto.Detalles)
        {
            if (item.Cantidad <= 0)
                return (false, $"La cantidad del material ID {item.MaterialId} debe ser mayor a cero.", null);

            var material = await _materialesRepo.GetByIdAsync(item.MaterialId);
            if (material is null || !material.Activo)
                return (false, $"El material con ID {item.MaterialId} no existe o está inactivo.", null);

            // Valida stock suficiente en AlmacenCentral
            var stockCentral = await _materialesRepo.GetStockCentralAsync(item.MaterialId);
            if (stockCentral is null || stockCentral.Stock < item.Cantidad)
                return (false, $"Stock insuficiente para '{material.Nombre}'. " +
                    $"Disponible: {stockCentral?.Stock ?? 0}, Solicitado: {item.Cantidad}.", null);

            // Descuenta AlmacenCentral en memoria (Change Tracker lo detecta)
            stockCentral.Stock -= item.Cantidad;
            stockCentral.UltimaActualizacion = DateTime.UtcNow;

            // Actualiza o crea AlmacenProyecto en memoria
            var stockProyecto = await _salidasRepo.GetStockProyectoAsync(dto.ProyectoId, item.MaterialId);
            if (stockProyecto is null)
            {
                _salidasRepo.TrackNuevoStockProyecto(new AlmacenProyecto
                {
                    ProyectoId = dto.ProyectoId,
                    MaterialId = item.MaterialId,
                    Stock = item.Cantidad,
                    UltimaActualizacion = DateTime.UtcNow
                });
            }
            else
            {
                stockProyecto.Stock += item.Cantidad;
                stockProyecto.UltimaActualizacion = DateTime.UtcNow;
            }

            detalles.Add(new SalidaDetalle
            {
                MaterialId = item.MaterialId,
                Cantidad = item.Cantidad
            });
        }

        var salida = new Salida
        {
            NumeroFolio = dto.NumeroFolio,
            ProyectoId = dto.ProyectoId,
            UsuarioId = dto.UsuarioId,
            Fecha = DateTime.UtcNow,
            Observaciones = dto.Observaciones,
            Detalles = detalles
        };

        // Único SaveChangesAsync: guarda Salida + Detalles + AlmacenCentral - stock + AlmacenProyecto + stock
        await _salidasRepo.RegistrarSalidaAsync(salida);

        return (true, "Salida registrada correctamente.", salida);
    }
}
