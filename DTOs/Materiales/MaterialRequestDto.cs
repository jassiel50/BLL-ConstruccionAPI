namespace BLL_ConstruccionAPI.DTOs.Materiales;

public class MaterialRequestDto
{
    public string Nombre { get; set; } = string.Empty;
    public string Descripcion { get; set; } = string.Empty;
    public string Codigo { get; set; } = string.Empty;
    public int CategoriaId { get; set; }
    public int UnidadMedidaId { get; set; }
    public decimal StockMinimo { get; set; }
    public decimal PrecioUnitario { get; set; }
}
