using BLL_ConstruccionAPI.DTOs.Bitacora;

namespace BLL_ConstruccionAPI.Services.Interfaces;

public interface IBitacoraService
{
    Task RegistrarAsync(int usuarioId, string nombreUsuario, string accion, string entidad, string descripcion, string? ip = null);
    Task<IEnumerable<BitacoraActividadDto>> GetAllAsync();
    Task<IEnumerable<BitacoraActividadDto>> GetByUsuarioAsync(int usuarioId);
}
