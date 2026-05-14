namespace BLL_ConstruccionAPI.DTOs.Pagos;

public class PagoClienteDto
{
    public int Id { get; set; }
    public int ProyectoId { get; set; }
    public string NombreProyecto { get; set; } = string.Empty;
    public string Concepto { get; set; } = string.Empty;
    public decimal Monto { get; set; }
    public DateTime FechaPago { get; set; }
    public string MetodoPago { get; set; } = string.Empty;
    public string Referencia { get; set; } = string.Empty;
    public string Notas { get; set; } = string.Empty;
    public DateTime FechaRegistro { get; set; }
}

public class PagoClienteRequestDto
{
    public string Concepto { get; set; } = string.Empty;
    public decimal Monto { get; set; }
    public DateTime FechaPago { get; set; }
    public string MetodoPago { get; set; } = string.Empty;
    public string Referencia { get; set; } = string.Empty;
    public string Notas { get; set; } = string.Empty;
}

public class ResumenPagosDto
{
    public int ProyectoId { get; set; }
    public string NombreProyecto { get; set; } = string.Empty;
    public decimal MontoContrato { get; set; }
    public decimal TotalPagado { get; set; }
    public decimal SaldoPendiente { get; set; }
    public int NumeroPagos { get; set; }
    public List<PagoClienteDto> Pagos { get; set; } = [];
}
