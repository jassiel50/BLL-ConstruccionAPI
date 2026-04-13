using BLL_ConstruccionAPI.Models.Inventario.Materiales;

namespace BLL_ConstruccionAPI.DTOs.Entradas;

public class EntradaDetalleResponseDto
{
    public int MaterialId { get; set; }
    public string NombreMaterial { get; set; } = string.Empty;
    public string CodigoMaterial { get; set; } = string.Empty;
    public decimal Cantidad { get; set; }
    public decimal PrecioUnitario { get; set; }
    public decimal Subtotal { get; set; }
    public string Zona { get; set; } = string.Empty;
    public string TipoUbicacion { get; set; } = string.Empty;

    public static EntradaDetalleResponseDto FromEntity(EntradaDetalle e) => new()
    {
        MaterialId = e.MaterialId,
        NombreMaterial = e.Material?.Nombre ?? string.Empty,
        CodigoMaterial = e.Material?.Codigo ?? string.Empty,
        Cantidad = e.Cantidad,
        PrecioUnitario = e.PrecioUnitario,
        Subtotal = e.Subtotal,
        Zona = e.Zona.ToString(),
        TipoUbicacion = e.TipoUbicacion.ToString()
    };
}
