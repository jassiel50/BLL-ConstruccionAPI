using System.ComponentModel.DataAnnotations;

namespace BLL_ConstruccionAPI.DTOs.ProveedoresClientes;

public class ContactoRequestDto
{
    [Required(ErrorMessage = "El nombre del contacto es obligatorio.")]
    [StringLength(100, MinimumLength = 1, ErrorMessage = "El nombre debe tener entre 1 y 100 caracteres.")]
    public string Nombre { get; set; } = string.Empty;

    [StringLength(20, ErrorMessage = "El teléfono no puede superar los 20 caracteres.")]
    public string Telefono { get; set; } = string.Empty;

    [EmailAddress(ErrorMessage = "El correo electrónico no tiene un formato válido.")]
    [StringLength(100, ErrorMessage = "El correo no puede superar los 100 caracteres.")]
    public string Email { get; set; } = string.Empty;

    [StringLength(100, ErrorMessage = "El cargo no puede superar los 100 caracteres.")]
    public string Cargo { get; set; } = string.Empty;

    public bool EsPrincipal { get; set; } = false;
}
