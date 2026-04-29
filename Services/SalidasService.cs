using BLL_ConstruccionAPI.DTOs.Materiales;
using BLL_ConstruccionAPI.DTOs.Salidas;
using BLL_ConstruccionAPI.Models.Inventario.Materiales;
using BLL_ConstruccionAPI.Repositories.Interfaces;
using BLL_ConstruccionAPI.Services.Interfaces;
using System.Security.Claims;

namespace BLL_ConstruccionAPI.Services;

public class SalidasService : ISalidasService
{
    private readonly ISalidasRepository _salidasRepo;
    private readonly IMaterialesRepository _materialesRepo;
    private readonly IProyectosRepository _proyectosRepo;
    private readonly IBitacoraService _bitacora;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public SalidasService(
        ISalidasRepository salidasRepo,
        IMaterialesRepository materialesRepo,
        IProyectosRepository proyectosRepo,
        IBitacoraService bitacora,
        IHttpContextAccessor httpContextAccessor)
    {
        _salidasRepo = salidasRepo;
        _materialesRepo = materialesRepo;
        _proyectosRepo = proyectosRepo;
        _bitacora = bitacora;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<IEnumerable<SalidaResponseDto>> GetAllAsync()
    {
        var salidas = await _salidasRepo.GetAllAsync();
        return salidas.Select(SalidaResponseDto.FromEntity);
    }

    public async Task<IEnumerable<SalidaResponseDto>> GetByProyectoAsync(int proyectoId)
    {
        var salidas = await _salidasRepo.GetByProyectoAsync(proyectoId);
        return salidas.Select(SalidaResponseDto.FromEntity);
    }

    public async Task<SalidaResponseDto?> GetByIdAsync(int id)
    {
        var salida = await _salidasRepo.GetByIdAsync(id);
        return salida is null ? null : SalidaResponseDto.FromEntity(salida);
    }

    public async Task<IEnumerable<AlmacenProyectoResponseDto>> GetAlmacenProyectoAsync(int proyectoId)
    {
        var almacen = await _salidasRepo.GetAlmacenProyectoAsync(proyectoId);
        return almacen.Select(AlmacenProyectoResponseDto.FromEntity);
    }

    public async Task<(bool Success, string Message, SalidaResponseDto? Data)> RegistrarAsync(SalidaRequestDto dto, int usuarioId)
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
                    Zona = stockCentral.Zona,
                    TipoUbicacion = stockCentral.TipoUbicacion,
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
            UsuarioId = usuarioId,
            Fecha = DateTime.UtcNow,
            Observaciones = dto.Observaciones,
            Detalles = detalles
        };

        // Único SaveChangesAsync: guarda Salida + Detalles + AlmacenCentral - stock + AlmacenProyecto + stock
        await _salidasRepo.RegistrarSalidaAsync(salida);

        var (uid, uname) = GetUsuarioInfo();
        await _bitacora.RegistrarAsync(uid, uname, "Registró", "Salida", $"Salida folio '{salida.NumeroFolio}' al proyecto '{proyecto.Nombre}'");

        return (true, "Salida registrada correctamente.", SalidaResponseDto.FromEntity(salida));
    }

    public async Task<(bool Success, string Message)> DevolverMaterialesAsync(int proyectoId, DevolucionRequestDto dto)
    {
        var proyecto = await _proyectosRepo.GetByIdAsync(proyectoId);
        if (proyecto is null)
            return (false, "El proyecto especificado no existe.");

        if (!dto.Detalles.Any())
            return (false, "La devolución debe tener al menos un material.");

        foreach (var item in dto.Detalles)
        {
            if (item.Cantidad <= 0)
                return (false, $"La cantidad del material ID {item.MaterialId} debe ser mayor a cero.");

            var material = await _materialesRepo.GetByIdAsync(item.MaterialId);
            if (material is null)
                return (false, $"El material con ID {item.MaterialId} no existe.");

            var stockProyecto = await _salidasRepo.GetStockProyectoAsync(proyectoId, item.MaterialId);
            if (stockProyecto is null || stockProyecto.Stock < item.Cantidad)
                return (false, $"Stock insuficiente en el proyecto para '{material.Nombre}'. " +
                    $"Disponible: {stockProyecto?.Stock ?? 0}, Solicitado: {item.Cantidad}.");

            var stockCentral = await _materialesRepo.GetStockCentralAsync(item.MaterialId);
            if (stockCentral is null)
                return (false, $"No se encontró el stock central para '{material.Nombre}'.");

            stockProyecto.Stock -= item.Cantidad;
            stockProyecto.UltimaActualizacion = DateTime.UtcNow;

            stockCentral.Stock += item.Cantidad;
            stockCentral.UltimaActualizacion = DateTime.UtcNow;
        }

        await _salidasRepo.GuardarCambiosAsync();
        return (true, "Devolución registrada. El material fue reintegrado al almacén central.");
    }

    private (int UsuarioId, string NombreUsuario) GetUsuarioInfo()
    {
        var user = _httpContextAccessor.HttpContext?.User;
        var id = int.TryParse(user?.FindFirstValue(ClaimTypes.NameIdentifier), out var parsed) ? parsed : 0;
        var nombre = user?.FindFirstValue("nombreUsuario") ?? "Sistema";
        return (id, nombre);
    }
}
