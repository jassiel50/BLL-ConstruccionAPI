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

    // Nulo hasta que el técnico lo define desde la liga pública.
    public TipoServicio? Tipo { get; set; }
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

    // Firma de quien solicita el trabajo — se captura al crear el servicio
    public string? NombreSolicitante { get; set; }
    public string? FirmaSolicitanteBase64 { get; set; }
    public DateTime? FechaFirmaSolicitante { get; set; }

    // Datos del Servicio (machote de reporte)
    public string? HorarioTrabajo { get; set; }
    public int? NumeroTrabajadores { get; set; }
    public decimal? TotalHorasTrabajadas { get; set; }

    // Recursos Utilizados
    public string? RecursoManoDeObra { get; set; }
    public string? RecursoHerramienta { get; set; }
    public string? RecursoRefacciones { get; set; }
    public string? RecursoConsumibles { get; set; }

    // Tipo de Trabajo — CSV de disciplinas seleccionadas (aditivo, no reemplaza Tipo)
    public string? TiposTrabajo { get; set; }

    public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;

    // Liga pública de un solo uso para llenado sin sesión (operador de servicio externo)
    public string? TokenPublico { get; set; }
    public DateTime? TokenExpira { get; set; }

    // Navegación
    public Cliente? Cliente { get; set; }
    public ICollection<ServicioFoto> Fotos { get; set; } = new List<ServicioFoto>();
}
