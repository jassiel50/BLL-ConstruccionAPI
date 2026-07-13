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

    /// <summary>Correo adicional opcional para recibir notificaciones/reportes (no se usa para iniciar sesión).</summary>
    [EmailAddress]
    [StringLength(100)]
    public string? EmailSecundario { get; set; }

    [Range(1, int.MaxValue)]
    public int RolId { get; set; }
}
