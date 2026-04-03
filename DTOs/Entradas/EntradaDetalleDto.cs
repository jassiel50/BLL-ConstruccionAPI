using System.ComponentModel.DataAnnotations;

namespace BLL_ConstruccionAPI.DTOs.Entradas;

public class EntradaDetalleDto
{
    [Range(1, int.MaxValue)]
    public int MaterialId { get; set; }

    [Range(0.01, double.MaxValue)]
    public decimal Cantidad { get; set; }

    [Range(0.01, double.MaxValue)]
    public decimal PrecioUnitario { get; set; }
}
