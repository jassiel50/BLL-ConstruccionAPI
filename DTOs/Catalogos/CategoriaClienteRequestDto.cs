using System.ComponentModel.DataAnnotations;

namespace BLL_ConstruccionAPI.DTOs.Catalogos;

public class CategoriaClienteRequestDto
{
    [Required]
    [StringLength(100, MinimumLength = 1)]
    public string Nombre { get; set; } = string.Empty;

    [StringLength(500)]
    public string Descripcion { get; set; } = string.Empty;
}
