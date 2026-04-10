using System.ComponentModel.DataAnnotations;

namespace BLL_ConstruccionAPI.DTOs.Herramientas;

public class HerramientaRequestDto
{
    [Required]
    [StringLength(150, MinimumLength = 1)]
    public string Nombre { get; set; } = string.Empty;

    [StringLength(500)]
    public string Descripcion { get; set; } = string.Empty;

    [Required]
    [StringLength(50, MinimumLength = 1)]
    public string Codigo { get; set; } = string.Empty;

    [Required]
    [StringLength(100, MinimumLength = 1)]
    public string NumeroSerie { get; set; } = string.Empty;

    [Range(1, int.MaxValue)]
    public int CategoriaHerramientaId { get; set; }

    [Required]
    public string Estado { get; set; } = "Disponible";

    [Range(0, double.MaxValue)]
    public decimal ValorAdquisicion { get; set; }

    public DateTime FechaAdquisicion { get; set; }

    [Required]
    public string Zona { get; set; } = "Guadalajara";

    [Required]
    public string TipoUbicacion { get; set; } = "Almacen";
}
