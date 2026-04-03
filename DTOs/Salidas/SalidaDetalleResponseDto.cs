using BLL_ConstruccionAPI.Models.Inventario.Materiales;

namespace BLL_ConstruccionAPI.DTOs.Salidas;

public class SalidaDetalleResponseDto
{
    public int MaterialId { get; set; }
    public string NombreMaterial { get; set; } = string.Empty;
    public string CodigoMaterial { get; set; } = string.Empty;
    public decimal Cantidad { get; set; }

    public static SalidaDetalleResponseDto FromEntity(SalidaDetalle e) => new()
    {
        MaterialId = e.MaterialId,
        NombreMaterial = e.Material?.Nombre ?? string.Empty,
        CodigoMaterial = e.Material?.Codigo ?? string.Empty,
        Cantidad = e.Cantidad
    };
}
