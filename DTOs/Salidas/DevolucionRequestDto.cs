using System.ComponentModel.DataAnnotations;

namespace BLL_ConstruccionAPI.DTOs.Salidas;

public class DevolucionRequestDto
{
    [Required]
    [MinLength(1, ErrorMessage = "La devolución debe tener al menos un material.")]
    public List<DevolucionDetalleDto> Detalles { get; set; } = [];

    [StringLength(500)]
    public string Observaciones { get; set; } = string.Empty;
}
