using BLL_ConstruccionAPI.DTOs.Entradas;
using BLL_ConstruccionAPI.Models.Inventario.Materiales;
using BLL_ConstruccionAPI.Repositories.Interfaces;
using BLL_ConstruccionAPI.Services.Interfaces;

namespace BLL_ConstruccionAPI.Services;

public class EntradasService : IEntradasService
{
    private readonly IEntradasRepository _entradasRepo;
    private readonly IMaterialesRepository _materialesRepo;
    private readonly IProveedoresClientesRepository _provClientesRepo;

    public EntradasService(
        IEntradasRepository entradasRepo,
        IMaterialesRepository materialesRepo,
        IProveedoresClientesRepository provClientesRepo)
    {
        _entradasRepo = entradasRepo;
        _materialesRepo = materialesRepo;
        _provClientesRepo = provClientesRepo;
    }

    public async Task<IEnumerable<Entrada>> GetAllAsync()
        => await _entradasRepo.GetAllAsync();

    public async Task<Entrada?> GetByIdAsync(int id)
        => await _entradasRepo.GetByIdAsync(id);

    public async Task<(bool Success, string Message, Entrada? Data)> RegistrarAsync(EntradaRequestDto dto)
    {
        if (!dto.Detalles.Any())
            return (false, "La entrada debe tener al menos un detalle.", null);

        if (await _entradasRepo.ExisteFolioAsync(dto.NumeroFolio))
            return (false, $"Ya existe una entrada con el folio '{dto.NumeroFolio}'.", null);

        var proveedor = await _provClientesRepo.GetProveedorByIdAsync(dto.ProveedorId);
        if (proveedor is null || !proveedor.Activo)
            return (false, "El proveedor especificado no existe o está inactivo.", null);

        var detalles = new List<EntradaDetalle>();
        decimal total = 0;

        foreach (var item in dto.Detalles)
        {
            if (item.Cantidad <= 0)
                return (false, $"La cantidad del material ID {item.MaterialId} debe ser mayor a cero.", null);

            if (item.PrecioUnitario <= 0)
                return (false, $"El precio unitario del material ID {item.MaterialId} debe ser mayor a cero.", null);

            var material = await _materialesRepo.GetByIdAsync(item.MaterialId);
            if (material is null || !material.Activo)
                return (false, $"El material con ID {item.MaterialId} no existe o está inactivo.", null);

            var subtotal = item.Cantidad * item.PrecioUnitario;
            total += subtotal;

            detalles.Add(new EntradaDetalle
            {
                MaterialId = item.MaterialId,
                Cantidad = item.Cantidad,
                PrecioUnitario = item.PrecioUnitario,
                Subtotal = subtotal
            });

            // Carga el registro de AlmacenCentral al Change Tracker y actualiza en memoria.
            // Como todos los repositorios comparten el mismo DbContext (scoped),
            // este cambio se guardará atómicamente en RegistrarEntradaAsync.
            var stockCentral = await _materialesRepo.GetStockCentralAsync(item.MaterialId);
            if (stockCentral is not null)
            {
                stockCentral.Stock += item.Cantidad;
                stockCentral.UltimaActualizacion = DateTime.UtcNow;
            }
        }

        var entrada = new Entrada
        {
            NumeroFolio = dto.NumeroFolio,
            ProveedorId = dto.ProveedorId,
            UsuarioId = dto.UsuarioId,
            Fecha = DateTime.UtcNow,
            Observaciones = dto.Observaciones,
            Total = total,
            Detalles = detalles
        };

        // Único SaveChangesAsync: guarda Entrada + Detalles + AlmacenCentral actualizado
        await _entradasRepo.RegistrarEntradaAsync(entrada);

        return (true, "Entrada registrada correctamente.", entrada);
    }
}
