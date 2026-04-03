using System.ComponentModel.DataAnnotations;

namespace BLL_ConstruccionAPI.DTOs.Salidas;

public class SalidaRequestDto
{
    [Required]
    [StringLength(50, MinimumLength = 1)]
    public string NumeroFolio { get; set; } = string.Empty;

    [Range(1, int.MaxValue)]
    public int ProyectoId { get; set; }

    [StringLength(500)]
    public string Observaciones { get; set; } = string.Empty;

    [Required]
    [MinLength(1, ErrorMessage = "La salida debe tener al menos un detalle.")]
    public List<SalidaDetalleDto> Detalles { get; set; } = [];
}
