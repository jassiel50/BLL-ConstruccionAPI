using System.ComponentModel.DataAnnotations;

namespace BLL_ConstruccionAPI.DTOs.Entradas;

public class EntradaRequestDto
{
    [Required]
    [StringLength(50, MinimumLength = 1)]
    public string NumeroFolio { get; set; } = string.Empty;

    [Range(1, int.MaxValue)]
    public int ProveedorId { get; set; }

    [StringLength(500)]
    public string Observaciones { get; set; } = string.Empty;

    [Required]
    [MinLength(1, ErrorMessage = "La entrada debe tener al menos un detalle.")]
    public List<EntradaDetalleDto> Detalles { get; set; } = [];
}
