namespace BLL_ConstruccionAPI.DTOs.GastosExtras;

public class GastoExtraDto
{
    public int Id { get; set; }
    public int FaseId { get; set; }
    public string Concepto { get; set; } = string.Empty;
    public decimal Monto { get; set; }
    public DateTime Fecha { get; set; }
    public string Observaciones { get; set; } = string.Empty;
}

public class GastoExtraRequestDto
{
    public string Concepto { get; set; } = string.Empty;
    public decimal Monto { get; set; }
    public DateTime Fecha { get; set; } = DateTime.UtcNow;
    public string Observaciones { get; set; } = string.Empty;
}
