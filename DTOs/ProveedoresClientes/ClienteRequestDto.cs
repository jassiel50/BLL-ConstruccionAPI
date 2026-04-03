using System.ComponentModel.DataAnnotations;

namespace BLL_ConstruccionAPI.DTOs.ProveedoresClientes;

public class ClienteRequestDto
{
    [Required]
    [StringLength(200, MinimumLength = 1)]
    public string Nombre { get; set; } = string.Empty;

    [Required]
    [StringLength(13, MinimumLength = 12)]
    public string RFC { get; set; } = string.Empty;

    [StringLength(100)]
    public string Contacto { get; set; } = string.Empty;

    [StringLength(20)]
    public string Telefono { get; set; } = string.Empty;

    [EmailAddress]
    [StringLength(100)]
    public string Email { get; set; } = string.Empty;

    [StringLength(300)]
    public string Direccion { get; set; } = string.Empty;
}
