namespace BLL_ConstruccionAPI.DTOs.ProveedoresClientes;

public class ClienteRequestDto
{
    public string Nombre { get; set; } = string.Empty;
    public string RFC { get; set; } = string.Empty;
    public string Contacto { get; set; } = string.Empty;
    public string Telefono { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Direccion { get; set; } = string.Empty;
}
