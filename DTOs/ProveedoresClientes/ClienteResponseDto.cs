using BLL_ConstruccionAPI.Models.Inventario;

namespace BLL_ConstruccionAPI.DTOs.ProveedoresClientes;

public class ClienteResponseDto
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string RFC { get; set; } = string.Empty;
    public string Contacto { get; set; } = string.Empty;
    public string Telefono { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Direccion { get; set; } = string.Empty;

    public static ClienteResponseDto FromEntity(Cliente e) => new()
    {
        Id = e.Id,
        Nombre = e.Nombre,
        RFC = e.RFC,
        Contacto = e.Contacto,
        Telefono = e.Telefono,
        Email = e.Email,
        Direccion = e.Direccion
    };
}
