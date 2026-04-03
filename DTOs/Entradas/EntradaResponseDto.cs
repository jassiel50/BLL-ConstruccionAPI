using BLL_ConstruccionAPI.Models.Inventario.Materiales;

namespace BLL_ConstruccionAPI.DTOs.Entradas;

public class EntradaResponseDto
{
    public int Id { get; set; }
    public string NumeroFolio { get; set; } = string.Empty;
    public int ProveedorId { get; set; }
    public string NombreProveedor { get; set; } = string.Empty;
    public int UsuarioId { get; set; }
    public DateTime Fecha { get; set; }
    public string Observaciones { get; set; } = string.Empty;
    public decimal Total { get; set; }
    public List<EntradaDetalleResponseDto> Detalles { get; set; } = [];

    public static EntradaResponseDto FromEntity(Entrada e) => new()
    {
        Id = e.Id,
        NumeroFolio = e.NumeroFolio,
        ProveedorId = e.ProveedorId,
        NombreProveedor = e.Proveedor?.Nombre ?? string.Empty,
        UsuarioId = e.UsuarioId,
        Fecha = e.Fecha,
        Observaciones = e.Observaciones,
        Total = e.Total,
        Detalles = e.Detalles.Select(EntradaDetalleResponseDto.FromEntity).ToList()
    };
}
