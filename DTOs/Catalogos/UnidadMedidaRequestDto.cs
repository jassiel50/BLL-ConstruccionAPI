using System.ComponentModel.DataAnnotations;

namespace BLL_ConstruccionAPI.DTOs.Catalogos;

public class UnidadMedidaRequestDto
{
    [Required]
    [StringLength(50, MinimumLength = 1)]
    public string Nombre { get; set; } = string.Empty;

    [Required]
    [StringLength(10, MinimumLength = 1)]
    public string Abreviatura { get; set; } = string.Empty;
}
