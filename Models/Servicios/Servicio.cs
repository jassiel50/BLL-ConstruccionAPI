using BLL_ConstruccionAPI.Models.Enums;
using BLL_ConstruccionAPI.Models.Inventario;

namespace BLL_ConstruccionAPI.Models.Servicios;

public class Servicio
{
    public int Id { get; set; }

    // Cliente: del catálogo (ClienteId) o capturado libremente en el momento
    public int? ClienteId { get; set; }
    public string ClienteNombre { get; set; } = string.Empty;
    public string ClienteDireccion { get; set; } = string.Empty;
    public string ClienteTelefono { get; set; } = string.Empty;

    public TipoServicio Tipo { get; set; }
    public string Equipo { get; set; } = string.Empty;
    public string DireccionServicio { get; set; } = string.Empty;
    public string DescripcionTrabajo { get; set; } = string.Empty;
    public string MaterialesUtilizados { get; set; } = string.Empty;
    public string Observaciones { get; set; } = string.Empty;

    public EstadoServicio Estado { get; set; } = EstadoServicio.Activo;

    public int OperadorId { get; set; }
    public string OperadorNombre { get; set; } = string.Empty;

    public DateTime FechaInicio { get; set; } = DateTime.UtcNow;
    public DateTime? FechaFin { get; set; }

    // Firma del cliente — se captura al finalizar el servicio y lo bloquea
    public string? FirmaBase64 { get; set; }
    public string? NombreQuienFirma { get; set; }
    public DateTime? FechaFirma { get; set; }

    public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;

    // Navegación
    public Cliente? Cliente { get; set; }
}
