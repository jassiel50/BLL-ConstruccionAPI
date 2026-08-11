namespace BLL_ConstruccionAPI.DTOs.Servicios;

public class ServicioRequestDto
{
    public int? ClienteId { get; set; }
    public string ClienteNombre { get; set; } = string.Empty;
    public string ClienteDireccion { get; set; } = string.Empty;
    public string ClienteTelefono { get; set; } = string.Empty;

    // Vacío hasta que el técnico lo define desde la liga pública.
    public string Tipo { get; set; } = string.Empty; // Instalacion, Mantenimiento, Reparacion, Otro
    public string Equipo { get; set; } = string.Empty;
    public string DireccionServicio { get; set; } = string.Empty;
    public string DescripcionTrabajo { get; set; } = string.Empty;
    public string MaterialesUtilizados { get; set; } = string.Empty;
    public string Observaciones { get; set; } = string.Empty;

    // Datos del Servicio
    public string? HorarioTrabajo { get; set; }
    public int? NumeroTrabajadores { get; set; }
    public decimal? TotalHorasTrabajadas { get; set; }

    // Recursos Utilizados
    public string? RecursoManoDeObra { get; set; }
    public string? RecursoHerramienta { get; set; }
    public string? RecursoRefacciones { get; set; }
    public string? RecursoConsumibles { get; set; }

    // Tipo de Trabajo (disciplinas seleccionadas)
    public List<string> TiposTrabajo { get; set; } = [];
}
