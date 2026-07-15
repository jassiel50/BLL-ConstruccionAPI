using BLL_ConstruccionAPI.DTOs.Herramientas;
using BLL_ConstruccionAPI.Models.Enums;
using BLL_ConstruccionAPI.Models.Inventario.Herramientas;
using BLL_ConstruccionAPI.Repositories.Interfaces;
using BLL_ConstruccionAPI.Services.Interfaces;
using System.Security.Claims;

namespace BLL_ConstruccionAPI.Services;

public class HerramientasService : IHerramientasService
{
    private readonly IHerramientasRepository _herramientasRepo;
    private readonly ICatalogosRepository _catalogosRepo;
    private readonly IProyectosRepository _proyectosRepo;
    private readonly IBitacoraService _bitacora;
    private readonly IHttpContextAccessor _httpContextAccessor;

    private static readonly EstadoHerramienta[] EstadosValidos =
        [EstadoHerramienta.Disponible, EstadoHerramienta.Asignada, EstadoHerramienta.Mantenimiento, EstadoHerramienta.Baja];

    public HerramientasService(
        IHerramientasRepository herramientasRepo,
        ICatalogosRepository catalogosRepo,
        IProyectosRepository proyectosRepo,
        IBitacoraService bitacora,
        IHttpContextAccessor httpContextAccessor)
    {
        _herramientasRepo = herramientasRepo;
        _catalogosRepo = catalogosRepo;
        _proyectosRepo = proyectosRepo;
        _bitacora = bitacora;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<IEnumerable<HerramientaResponseDto>> GetAllAsync()
    {
        var herramientas = await _herramientasRepo.GetAllAsync();
        var result = new List<HerramientaResponseDto>();
        foreach (var h in herramientas)
        {
            var asignadas = await _herramientasRepo.GetCantidadAsignadaAsync(h.Id);
            result.Add(HerramientaResponseDto.FromEntity(h, h.Cantidad - asignadas));
        }
        return result;
    }

    public async Task<IEnumerable<HerramientaResponseDto>> GetDisponiblesAsync()
    {
        var herramientas = await _herramientasRepo.GetAllAsync();
        var result = new List<HerramientaResponseDto>();
        foreach (var h in herramientas.Where(h => h.Activo))
        {
            var asignadas = await _herramientasRepo.GetCantidadAsignadaAsync(h.Id);
            var disponibles = h.Cantidad - asignadas;
            if (disponibles > 0)
                result.Add(HerramientaResponseDto.FromEntity(h, disponibles));
        }
        return result;
    }

    public async Task<HerramientaResponseDto?> GetByIdAsync(int id)
    {
        var herramienta = await _herramientasRepo.GetByIdAsync(id);
        if (herramienta is null) return null;
        var asignadas = await _herramientasRepo.GetCantidadAsignadaAsync(herramienta.Id);
        return HerramientaResponseDto.FromEntity(herramienta, herramienta.Cantidad - asignadas);
    }

    public async Task<IEnumerable<AsignacionHerramientaResponseDto>> GetAsignacionesAsync(int herramientaId)
    {
        var asignaciones = await _herramientasRepo.GetAsignacionesByHerramientaAsync(herramientaId);
        return asignaciones.Select(AsignacionHerramientaResponseDto.FromEntity);
    }

    public async Task<(bool Success, string Message, HerramientaResponseDto? Data)> CreateAsync(HerramientaRequestDto dto)
    {
        if (!Enum.TryParse<EstadoHerramienta>(dto.Estado, out var estadoHerramienta))
            return (false, $"Estado inválido. Valores permitidos: {string.Join(", ", Enum.GetNames<EstadoHerramienta>())}.", null);

        if (!Enum.TryParse<Zona>(dto.Zona, out var zona))
            return (false, $"Zona inválida. Valores permitidos: {string.Join(", ", Enum.GetNames<Zona>())}.", null);

        if (string.IsNullOrWhiteSpace(dto.TipoUbicacion))
            return (false, "TipoUbicacion es requerido.", null);

        if (await _herramientasRepo.ExisteCodigoAsync(dto.Codigo))
            return (false, "Ya existe una herramienta con ese código.", null);

        if (await _herramientasRepo.ExisteNumeroSerieAsync(dto.NumeroSerie))
            return (false, "Ya existe una herramienta con ese número de serie.", null);

        var categoria = await _catalogosRepo.GetCategoriaHerramientaByIdAsync(dto.CategoriaHerramientaId);
        if (categoria is null || !categoria.Activo)
            return (false, "La categoría de herramienta especificada no existe o está inactiva.", null);

        var herramienta = new Herramienta
        {
            Nombre = dto.Nombre,
            Descripcion = dto.Descripcion,
            Codigo = dto.Codigo,
            NumeroSerie = dto.NumeroSerie,
            CategoriaHerramientaId = dto.CategoriaHerramientaId,
            Estado = estadoHerramienta,
            Zona = zona,
            TipoUbicacion = dto.TipoUbicacion.Trim(),
            ValorAdquisicion = dto.ValorAdquisicion,
            FechaAdquisicion = dto.FechaAdquisicion,
            Cantidad = dto.Cantidad,
            Activo = true,
            FechaRegistro = DateTime.UtcNow
        };

        await _herramientasRepo.CreateAsync(herramienta);
        var (uid, uname) = GetUsuarioInfo();
        await _bitacora.RegistrarAsync(uid, uname, "Creó", "Herramienta", $"Herramienta '{herramienta.Nombre}' creada");
        return (true, "Herramienta registrada correctamente.", HerramientaResponseDto.FromEntity(herramienta));
    }

    public async Task<List<HerramientaBulkResultDto>> CreateBulkAsync(List<HerramientaRequestDto> dtos)
    {
        var resultados = new List<HerramientaBulkResultDto>();

        foreach (var dto in dtos)
        {
            var (success, message, data) = await CreateAsync(dto);
            resultados.Add(new HerramientaBulkResultDto
            {
                Codigo = dto.Codigo,
                ResponseCode = success ? 1 : -2,
                ResponseMsg = message,
                Id = data?.Id
            });
        }

        return resultados;
    }

    public async Task<(bool Success, string Message)> UpdateAsync(int id, HerramientaRequestDto dto)
    {
        var herramienta = await _herramientasRepo.GetByIdAsync(id);
        if (herramienta is null) return (false, "Herramienta no encontrada.");

        if (!Enum.TryParse<EstadoHerramienta>(dto.Estado, out var estadoActualizado))
            return (false, $"Estado inválido. Valores permitidos: {string.Join(", ", Enum.GetNames<EstadoHerramienta>())}.");

        if (!Enum.TryParse<Zona>(dto.Zona, out var zonaActualizada))
            return (false, $"Zona inválida. Valores permitidos: {string.Join(", ", Enum.GetNames<Zona>())}.");

        if (string.IsNullOrWhiteSpace(dto.TipoUbicacion))
            return (false, "TipoUbicacion es requerido.");

        if (herramienta.Codigo != dto.Codigo && await _herramientasRepo.ExisteCodigoAsync(dto.Codigo))
            return (false, "Ya existe una herramienta con ese código.");

        if (herramienta.NumeroSerie != dto.NumeroSerie && await _herramientasRepo.ExisteNumeroSerieAsync(dto.NumeroSerie))
            return (false, "Ya existe una herramienta con ese número de serie.");

        var categoria = await _catalogosRepo.GetCategoriaHerramientaByIdAsync(dto.CategoriaHerramientaId);
        if (categoria is null || !categoria.Activo)
            return (false, "La categoría de herramienta especificada no existe o está inactiva.");

        herramienta.Nombre = dto.Nombre;
        herramienta.Descripcion = dto.Descripcion;
        herramienta.Codigo = dto.Codigo;
        herramienta.NumeroSerie = dto.NumeroSerie;
        herramienta.CategoriaHerramientaId = dto.CategoriaHerramientaId;
        herramienta.Estado = estadoActualizado;
        herramienta.Zona = zonaActualizada;
        herramienta.TipoUbicacion = dto.TipoUbicacion.Trim();
        herramienta.ValorAdquisicion = dto.ValorAdquisicion;
        herramienta.FechaAdquisicion = dto.FechaAdquisicion;
        herramienta.Cantidad = dto.Cantidad;

        await _herramientasRepo.UpdateAsync(herramienta);
        var (uid2, uname2) = GetUsuarioInfo();
        await _bitacora.RegistrarAsync(uid2, uname2, "Actualizó", "Herramienta", $"Herramienta '{herramienta.Nombre}' actualizada");
        return (true, "Herramienta actualizada correctamente.");
    }

    public async Task<(bool Success, string Message)> DeleteAsync(int id)
    {
        var herramienta = await _herramientasRepo.GetByIdAsync(id);
        if (herramienta is null) return (false, "Herramienta no encontrada.");

        if (herramienta.Estado == EstadoHerramienta.Asignada)
            return (false, "No se puede dar de baja una herramienta que está asignada a un proyecto.");

        await _herramientasRepo.DeleteAsync(herramienta);
        var (uid, uname) = GetUsuarioInfo();
        await _bitacora.RegistrarAsync(uid, uname, "Eliminó", "Herramienta", $"Herramienta '{herramienta.Nombre}' eliminada");
        return (true, "Herramienta dada de baja correctamente.");
    }

    public async Task<(bool Success, string Message, AsignacionHerramientaResponseDto? Data)> AsignarAsync(AsignacionRequestDto dto, int usuarioId)
    {
        var herramienta = await _herramientasRepo.GetByIdAsync(dto.HerramientaId);
        if (herramienta is null || !herramienta.Activo)
            return (false, "Herramienta no encontrada.", null);

        if (herramienta.Estado == EstadoHerramienta.Baja)
            return (false, "La herramienta está dada de baja.", null);

        var cantidadAsignada = await _herramientasRepo.GetCantidadAsignadaAsync(dto.HerramientaId);
        if (cantidadAsignada >= herramienta.Cantidad)
            return (false, $"No hay unidades disponibles de esta herramienta. Cantidad total: {herramienta.Cantidad}, asignadas: {cantidadAsignada}.", null);

        var proyecto = await _proyectosRepo.GetByIdAsync(dto.ProyectoId);
        if (proyecto is null)
            return (false, "El proyecto especificado no existe.", null);

        var asignacion = new AsignacionHerramienta
        {
            HerramientaId = dto.HerramientaId,
            ProyectoId = dto.ProyectoId,
            UsuarioAsignoId = usuarioId,
            UsuarioRecibeId = dto.UsuarioRecibeId,
            FechaAsignacion = DateTime.UtcNow,
            Estado = EstadoAsignacion.Asignada,
            Observaciones = dto.Observaciones
        };

        herramienta.Estado = (cantidadAsignada + 1 >= herramienta.Cantidad)
            ? EstadoHerramienta.Asignada
            : EstadoHerramienta.Disponible;
        await _herramientasRepo.AsignarHerramientaAsync(asignacion, herramienta);

        var (uid, uname) = GetUsuarioInfo();
        await _bitacora.RegistrarAsync(uid, uname, "Asignó", "Herramienta", $"'{herramienta.Nombre}' asignada al proyecto '{proyecto.Nombre}'");

        return (true, "Herramienta asignada correctamente.", AsignacionHerramientaResponseDto.FromEntity(asignacion));
    }

    public async Task<(bool Success, string Message)> DevolverAsync(int asignacionId, DevolucionRequestDto dto)
    {
        var asignacion = await _herramientasRepo.GetAsignacionByIdAsync(asignacionId);
        if (asignacion is null) return (false, "Asignación no encontrada.");

        if (asignacion.Estado != EstadoAsignacion.Asignada)
            return (false, "Esta asignación ya fue devuelta.");

        asignacion.Estado = EstadoAsignacion.Devuelta;
        asignacion.FechaDevolucion = DateTime.UtcNow;
        asignacion.ObservacionesDevolucion = dto.ObservacionesDevolucion;

        var herramienta = await _herramientasRepo.GetByIdAsync(asignacion.HerramientaId);
        if (herramienta is null)
            return (false, "La herramienta asociada a la asignación no fue encontrada.");

        // Después de marcar como devuelta, la cantidad asignada activa baja en 1
        var cantidadAsignadaTrasDevolucion = await _herramientasRepo.GetCantidadAsignadaAsync(herramienta.Id) - 1;
        herramienta.Estado = cantidadAsignadaTrasDevolucion > 0
            ? EstadoHerramienta.Asignada
            : EstadoHerramienta.Disponible;

        await _herramientasRepo.DevolverHerramientaAsync(asignacion, herramienta);

        var (uid, uname) = GetUsuarioInfo();
        await _bitacora.RegistrarAsync(uid, uname, "Devolvió", "Herramienta", $"'{herramienta.Nombre}' devuelta");

        return (true, "Herramienta devuelta correctamente.");
    }

    public async Task<IEnumerable<AsignacionActivaDto>> GetAsignacionesActivasAsync()
    {
        var asignaciones = await _herramientasRepo.GetAsignacionesActivasAsync();
        return asignaciones.Select(AsignacionActivaDto.FromEntity);
    }

    public async Task<List<AsignacionOperacionResultDto>> DevolverMultipleAsync(List<int> asignacionIds, string observacionesDevolucion)
    {
        var resultados = new List<AsignacionOperacionResultDto>();
        var dto = new DevolucionRequestDto { ObservacionesDevolucion = observacionesDevolucion };

        foreach (var id in asignacionIds)
        {
            var (success, message) = await DevolverAsync(id, dto);
            resultados.Add(new AsignacionOperacionResultDto
            {
                AsignacionId = id,
                ResponseCode = success ? 1 : -2,
                ResponseMsg = message
            });
        }

        return resultados;
    }

    public async Task<(bool Success, string Message)> TransferirAsync(int asignacionId, int nuevoProyectoId, int usuarioId)
    {
        var asignacion = await _herramientasRepo.GetAsignacionByIdAsync(asignacionId);
        if (asignacion is null) return (false, "Asignación no encontrada.");

        if (asignacion.Estado != EstadoAsignacion.Asignada)
            return (false, "Esta asignación ya fue devuelta.");

        if (asignacion.ProyectoId == nuevoProyectoId)
            return (false, "La herramienta ya está asignada a ese proyecto.");

        var proyectoDestino = await _proyectosRepo.GetByIdAsync(nuevoProyectoId);
        if (proyectoDestino is null)
            return (false, "El proyecto destino no existe.");

        var herramienta = await _herramientasRepo.GetByIdAsync(asignacion.HerramientaId);
        if (herramienta is null)
            return (false, "La herramienta asociada a la asignación no fue encontrada.");

        var proyectoOrigenNombre = asignacion.Proyecto?.Nombre ?? $"#{asignacion.ProyectoId}";

        asignacion.Estado = EstadoAsignacion.Devuelta;
        asignacion.FechaDevolucion = DateTime.UtcNow;
        asignacion.ObservacionesDevolucion = $"Transferida al proyecto '{proyectoDestino.Nombre}'.";

        var nuevaAsignacion = new AsignacionHerramienta
        {
            HerramientaId = herramienta.Id,
            ProyectoId = nuevoProyectoId,
            UsuarioAsignoId = usuarioId,
            UsuarioRecibeId = asignacion.UsuarioRecibeId,
            FechaAsignacion = DateTime.UtcNow,
            Estado = EstadoAsignacion.Asignada,
            Observaciones = $"Transferida desde el proyecto '{proyectoOrigenNombre}'."
        };

        await _herramientasRepo.TransferirHerramientaAsync(asignacion, nuevaAsignacion);

        var (uid, uname) = GetUsuarioInfo();
        await _bitacora.RegistrarAsync(uid, uname, "Transfirió", "Herramienta", $"'{herramienta.Nombre}' transferida del proyecto '{proyectoOrigenNombre}' a '{proyectoDestino.Nombre}'");

        return (true, "Herramienta transferida correctamente.");
    }

    public async Task<List<AsignacionOperacionResultDto>> TransferirMultipleAsync(List<int> asignacionIds, int nuevoProyectoId, int usuarioId)
    {
        var resultados = new List<AsignacionOperacionResultDto>();

        foreach (var id in asignacionIds)
        {
            var (success, message) = await TransferirAsync(id, nuevoProyectoId, usuarioId);
            resultados.Add(new AsignacionOperacionResultDto
            {
                AsignacionId = id,
                ResponseCode = success ? 1 : -2,
                ResponseMsg = message
            });
        }

        return resultados;
    }

    public async Task<(bool Success, string Message)> CambiarUbicacionAsync(int herramientaId, string tipoUbicacion)
    {
        if (string.IsNullOrWhiteSpace(tipoUbicacion))
            return (false, "TipoUbicacion es requerido.");

        var herramienta = await _herramientasRepo.GetByIdAsync(herramientaId);
        if (herramienta is null) return (false, "Herramienta no encontrada.");

        var nuevaUbicacion = tipoUbicacion.Trim();
        herramienta.TipoUbicacion = nuevaUbicacion;
        await _herramientasRepo.UpdateAsync(herramienta);

        var (uid, uname) = GetUsuarioInfo();
        await _bitacora.RegistrarAsync(uid, uname, "Actualizó", "Herramienta", $"'{herramienta.Nombre}' movida a {nuevaUbicacion}");

        return (true, "Ubicación actualizada correctamente.");
    }

    private (int UsuarioId, string NombreUsuario) GetUsuarioInfo()
    {
        var user = _httpContextAccessor.HttpContext?.User;
        var id = int.TryParse(user?.FindFirstValue(ClaimTypes.NameIdentifier), out var parsed) ? parsed : 0;
        var nombre = user?.FindFirstValue("nombreUsuario") ?? "Sistema";
        return (id, nombre);
    }
}
