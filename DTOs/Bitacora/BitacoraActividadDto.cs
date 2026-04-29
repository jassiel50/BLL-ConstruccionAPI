using BLL_ConstruccionAPI.Models.Auth;

namespace BLL_ConstruccionAPI.DTOs.Bitacora;

public class BitacoraActividadDto
{
    public int Id { get; set; }
    public int UsuarioId { get; set; }
    public string NombreUsuario { get; set; } = string.Empty;
    public string Accion { get; set; } = string.Empty;
    public string Entidad { get; set; } = string.Empty;
    public string Descripcion { get; set; } = string.Empty;
    public DateTime Fecha { get; set; }
    public string? IpOrigen { get; set; }

    public static BitacoraActividadDto FromEntity(BitacoraActividad b) => new()
    {
        Id            = b.Id,
        UsuarioId     = b.UsuarioId,
        NombreUsuario = b.NombreUsuario,
        Accion        = b.Accion,
        Entidad       = b.Entidad,
        Descripcion   = b.Descripcion,
        Fecha         = b.Fecha,
        IpOrigen      = b.IpOrigen
    };
}
