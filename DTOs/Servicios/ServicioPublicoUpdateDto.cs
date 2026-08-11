namespace BLL_ConstruccionAPI.DTOs.Servicios;

// Todos los campos que llena el técnico en campo a través de la liga pública.
// Solo el Cliente lo define el operador interno al crear el servicio.
public class ServicioPublicoUpdateDto
{
    public string? Tipo { get; set; } // Instalacion, Mantenimiento, Reparacion, Otro
    public string? DireccionServicio { get; set; }
    public string? NombreSolicitante { get; set; }

    public string Equipo { get; set; } = string.Empty;
    public string DescripcionTrabajo { get; set; } = string.Empty;
    public string MaterialesUtilizados { get; set; } = string.Empty;
    public string Observaciones { get; set; } = string.Empty;

    public string? HorarioTrabajo { get; set; }
    public int? NumeroTrabajadores { get; set; }
    public decimal? TotalHorasTrabajadas { get; set; }

    public string? RecursoManoDeObra { get; set; }
    public string? RecursoHerramienta { get; set; }
    public string? RecursoRefacciones { get; set; }
    public string? RecursoConsumibles { get; set; }

    public List<string> TiposTrabajo { get; set; } = [];
}
