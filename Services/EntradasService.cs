using BLL_ConstruccionAPI.DTOs.Entradas;
using BLL_ConstruccionAPI.Models.Enums;
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

    public async Task<IEnumerable<EntradaResponseDto>> GetAllAsync()
    {
        var entradas = await _entradasRepo.GetAllAsync();
        return entradas.Select(EntradaResponseDto.FromEntity);
    }

    public async Task<EntradaResponseDto?> GetByIdAsync(int id)
    {
        var entrada = await _entradasRepo.GetByIdAsync(id);
        return entrada is null ? null : EntradaResponseDto.FromEntity(entrada);
    }

    public async Task<(bool Success, string Message, EntradaResponseDto? Data)> RegistrarAsync(EntradaRequestDto dto, int usuarioId)
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

            if (!Enum.TryParse<Zona>(item.Zona, out var zona))
                return (false, $"Zona inválida para el material ID {item.MaterialId}. Valores permitidos: {string.Join(", ", Enum.GetNames<Zona>())}.", null);

            if (!Enum.TryParse<TipoUbicacion>(item.TipoUbicacion, out var tipoUbicacion))
                return (false, $"TipoUbicacion inválido para el material ID {item.MaterialId}. Valores permitidos: {string.Join(", ", Enum.GetNames<TipoUbicacion>())}.", null);

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
                Subtotal = subtotal,
                Zona = zona,
                TipoUbicacion = tipoUbicacion
            });

            // Carga el registro de AlmacenCentral al Change Tracker y actualiza en memoria.
            // Como todos los repositorios comparten el mismo DbContext (scoped),
            // este cambio se guardará atómicamente en RegistrarEntradaAsync.
            var stockCentral = await _materialesRepo.GetStockCentralAsync(item.MaterialId);
            if (stockCentral is null)
                return (false, $"No existe registro de almacén central para el material ID {item.MaterialId}.", null);

            stockCentral.Stock += item.Cantidad;
            stockCentral.Zona = zona;
            stockCentral.TipoUbicacion = tipoUbicacion;
            stockCentral.UltimaActualizacion = DateTime.UtcNow;
        }

        var entrada = new Entrada
        {
            NumeroFolio = dto.NumeroFolio,
            ProveedorId = dto.ProveedorId,
            UsuarioId = usuarioId,
            Fecha = DateTime.UtcNow,
            Observaciones = dto.Observaciones,
            Total = total,
            Detalles = detalles
        };

        // Único SaveChangesAsync: guarda Entrada + Detalles + AlmacenCentral actualizado
        await _entradasRepo.RegistrarEntradaAsync(entrada);

        return (true, "Entrada registrada correctamente.", EntradaResponseDto.FromEntity(entrada));
    }
}
