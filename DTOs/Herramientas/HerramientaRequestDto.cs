namespace BLL_ConstruccionAPI.DTOs.Herramientas;

public class HerramientaRequestDto
{
    public string Nombre { get; set; } = string.Empty;
    public string Descripcion { get; set; } = string.Empty;
    public string Codigo { get; set; } = string.Empty;
    public string NumeroSerie { get; set; } = string.Empty;
    public int CategoriaHerramientaId { get; set; }
    public string Estado { get; set; } = "Disponible";
    public decimal ValorAdquisicion { get; set; }
    public DateTime FechaAdquisicion { get; set; }
}
