using System.ComponentModel.DataAnnotations;

namespace BLL_ConstruccionAPI.DTOs.Materiales;

public class MaterialRequestDto
{
    [Required]
    [StringLength(150, MinimumLength = 1)]
    public string Nombre { get; set; } = string.Empty;

    [StringLength(500)]
    public string Descripcion { get; set; } = string.Empty;

    [Required]
    [StringLength(50, MinimumLength = 1)]
    public string Codigo { get; set; } = string.Empty;

    [Range(1, int.MaxValue)]
    public int CategoriaId { get; set; }

    [Range(1, int.MaxValue)]
    public int UnidadMedidaId { get; set; }

    [Range(0, double.MaxValue)]
    public decimal StockMinimo { get; set; }

    [Range(0, double.MaxValue)]
    public decimal PrecioUnitario { get; set; }

    [Required]
    public string Zona { get; set; } = "Guadalajara";

    [Required]
    public string TipoUbicacion { get; set; } = "Almacen";

    public bool EnProyecto { get; set; } = false;
}
