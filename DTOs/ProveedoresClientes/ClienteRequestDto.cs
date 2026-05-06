using System.ComponentModel.DataAnnotations;

namespace BLL_ConstruccionAPI.DTOs.ProveedoresClientes;

public class ClienteRequestDto
{
    [Required(ErrorMessage = "El nombre del cliente es obligatorio.")]
    [StringLength(200, MinimumLength = 1, ErrorMessage = "El nombre debe tener entre 1 y 200 caracteres.")]
    public string Nombre { get; set; } = string.Empty;

    [Required(ErrorMessage = "El RFC es obligatorio.")]
    [StringLength(20, ErrorMessage = "El RFC no puede superar los 20 caracteres.")]
    public string RFC { get; set; } = string.Empty;

    [StringLength(100, ErrorMessage = "El nombre del contacto no puede superar los 100 caracteres.")]
    public string Contacto { get; set; } = string.Empty;

    [StringLength(20, ErrorMessage = "El teléfono no puede superar los 20 caracteres.")]
    public string Telefono { get; set; } = string.Empty;

    [EmailAddress(ErrorMessage = "El correo electrónico no tiene un formato válido (ej. correo@dominio.com).")]
    [StringLength(100, ErrorMessage = "El correo no puede superar los 100 caracteres.")]
    public string Email { get; set; } = string.Empty;

    [StringLength(300, ErrorMessage = "La dirección no puede superar los 300 caracteres.")]
    public string Direccion { get; set; } = string.Empty;
}
