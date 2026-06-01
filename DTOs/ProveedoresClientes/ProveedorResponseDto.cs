using BLL_ConstruccionAPI.Models.Inventario;

namespace BLL_ConstruccionAPI.DTOs.ProveedoresClientes;

public class ProveedorResponseDto
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string RFC { get; set; } = string.Empty;
    public string Direccion { get; set; } = string.Empty;
    public string Estado { get; set; } = string.Empty;
    public string Descripcion { get; set; } = string.Empty;
    public int? CategoriaId { get; set; }
    public string? CategoriaNombre { get; set; }
    public List<ContactoResponseDto> Contactos { get; set; } = new();

    public static ProveedorResponseDto FromEntity(Proveedor e) => new()
    {
        Id = e.Id,
        Nombre = e.Nombre,
        RFC = e.RFC,
        Direccion = e.Direccion,
        Estado = e.Estado,
        Descripcion = e.Descripcion,
        CategoriaId = e.CategoriaId,
        CategoriaNombre = e.Categoria?.Nombre,
        Contactos = e.Contactos?.Select(c => new ContactoResponseDto
        {
            Id = c.Id,
            Nombre = c.Nombre,
            Telefono = c.Telefono,
            Email = c.Email ?? string.Empty,
            Cargo = c.Cargo,
            EsPrincipal = c.EsPrincipal
        }).ToList() ?? new()
    };
}
