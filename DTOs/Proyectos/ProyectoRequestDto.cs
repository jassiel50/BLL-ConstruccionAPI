using System.ComponentModel.DataAnnotations;

namespace BLL_ConstruccionAPI.DTOs.Proyectos;

public class ProyectoRequestDto
{
    [Required]
    [StringLength(150, MinimumLength = 1)]
    public string Nombre { get; set; } = string.Empty;

    [StringLength(500)]
    public string Descripcion { get; set; } = string.Empty;

    [Required]
    [StringLength(300, MinimumLength = 1)]
    public string Ubicacion { get; set; } = string.Empty;

    [Range(1, int.MaxValue)]
    public int ClienteId { get; set; }

    [Range(1, int.MaxValue)]
    public int ResponsableId { get; set; }

    public DateTime FechaInicio { get; set; }
    public DateTime? FechaFin { get; set; }

    [Required]
    public string Estado { get; set; } = "Activo";
}
