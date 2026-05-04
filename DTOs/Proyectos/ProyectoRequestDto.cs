using System.ComponentModel.DataAnnotations;

namespace BLL_ConstruccionAPI.DTOs.Proyectos;

public class ProyectoRequestDto
{
    [Required]
    [StringLength(150, MinimumLength = 1)]
    public string Nombre { get; set; } = string.Empty;

    [StringLength(500)]
    public string Descripcion { get; set; } = string.Empty;

    [StringLength(300)]
    public string Ubicacion { get; set; } = string.Empty;

    [Range(1, int.MaxValue)]
    public int ClienteId { get; set; }

    public int ResponsableId { get; set; }

    public DateTime FechaInicio { get; set; }
    public DateTime? FechaFin { get; set; }

    [Required]
    public string Estado { get; set; } = "Activo";

    [StringLength(100)]
    public string NumeroCotizacion { get; set; } = string.Empty;

    [StringLength(100)]
    public string OrdenCompra { get; set; } = string.Empty;

    [Range(0, double.MaxValue)]
    public decimal MontoContrato { get; set; } = 0;

    [Range(0, double.MaxValue)]
    public decimal PresupuestoEstimado { get; set; } = 0;
}
