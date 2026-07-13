using System.ComponentModel.DataAnnotations;

namespace BLL_ConstruccionAPI.DTOs.Reportes;

public class ConfiguracionReporteRequestDto
{
    [Required]
    [MaxLength(100)]
    public string Nombre { get; set; } = string.Empty;

    [Required]
    [RegularExpression("^(diario|semanal|mensual)$",
        ErrorMessage = "Frecuencia debe ser 'diario', 'semanal' o 'mensual'.")]
    public string Frecuencia { get; set; } = string.Empty;

    [Range(0, 23, ErrorMessage = "HoraEnvio debe estar entre 0 y 23.")]
    public int HoraEnvio { get; set; }

    [Required]
    [MinLength(1, ErrorMessage = "Debe incluir al menos una sección.")]
    public List<string> Secciones { get; set; } = [];

    /// <summary>Direcciones de correo destinatarias. Si viene vacía, se usa el correo del dueño de la configuración.</summary>
    public List<string> Destinatarios { get; set; } = [];

    /// <summary>Requerido solo cuando Secciones incluye "avance_cliente".</summary>
    public int? ProyectoId { get; set; }

    public bool Activo { get; set; } = true;
}
