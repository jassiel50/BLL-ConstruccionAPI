using BLL_ConstruccionAPI.Models.Inventario.Materiales;

namespace BLL_ConstruccionAPI.DTOs.Materiales;

public class MaterialResponseDto
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Descripcion { get; set; } = string.Empty;
    public string Codigo { get; set; } = string.Empty;
    public int CategoriaId { get; set; }
    public string NombreCategoria { get; set; } = string.Empty;
    public int UnidadMedidaId { get; set; }
    public string AbreviaturaUnidad { get; set; } = string.Empty;
    public decimal StockMinimo { get; set; }
    public decimal PrecioUnitario { get; set; }

    public static MaterialResponseDto FromEntity(Material e) => new()
    {
        Id = e.Id,
        Nombre = e.Nombre,
        Descripcion = e.Descripcion,
        Codigo = e.Codigo,
        CategoriaId = e.CategoriaId,
        NombreCategoria = e.Categoria?.Nombre ?? string.Empty,
        UnidadMedidaId = e.UnidadMedidaId,
        AbreviaturaUnidad = e.UnidadMedida?.Abreviatura ?? string.Empty,
        StockMinimo = e.StockMinimo,
        PrecioUnitario = e.PrecioUnitario
    };
}
