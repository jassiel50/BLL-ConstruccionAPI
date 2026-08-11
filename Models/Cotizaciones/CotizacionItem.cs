namespace BLL_ConstruccionAPI.Models.Cotizaciones;

public class CotizacionItem
{
    public int Id { get; set; }
    public int CotizacionId { get; set; }

    public int Orden { get; set; }
    public string Descripcion { get; set; } = string.Empty;
    // Texto libre: en la práctica no siempre es numérico (ej. "SET").
    public string Cantidad { get; set; } = string.Empty;
    public string Unidad { get; set; } = string.Empty;
    public decimal Total { get; set; }

    public Cotizacion? Cotizacion { get; set; }
}
