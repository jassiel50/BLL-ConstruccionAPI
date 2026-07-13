using System.ComponentModel.DataAnnotations;

namespace BLL_ConstruccionAPI.DTOs.Auth;

public class UsuarioUpdateDto
{
    [Required]
    [StringLength(100)]
    public string Nombre { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    [StringLength(100)]
    public string Email { get; set; } = string.Empty;

    [Range(1, int.MaxValue)]
    public int RolId { get; set; }
}
