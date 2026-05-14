namespace BLL_ConstruccionAPI.DTOs.GastosExtras;

public class GastoExtraDto
{
    public int Id { get; set; }
    public int FaseId { get; set; }
    public string Concepto { get; set; } = string.Empty;
    public decimal Monto { get; set; }
    public decimal MontoProveedor { get; set; }
    public decimal CobradoCliente { get; set; }
    public int? ProveedorId { get; set; }
    public string? NombreProveedor { get; set; }
    public DateTime Fecha { get; set; }
    public string Observaciones { get; set; } = string.Empty;
}

public class GastoExtraRequestDto
{
    public string Concepto { get; set; } = string.Empty;
    public decimal Monto { get; set; }
    public decimal MontoProveedor { get; set; } = 0;
    public decimal CobradoCliente { get; set; } = 0;
    public int? ProveedorId { get; set; }
    public DateTime Fecha { get; set; } = DateTime.UtcNow;
    public string Observaciones { get; set; } = string.Empty;
}
