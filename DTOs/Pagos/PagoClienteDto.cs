namespace BLL_ConstruccionAPI.DTOs.Pagos;

public class PagoClienteDto
{
    public int Id { get; set; }
    public int ProyectoId { get; set; }
    public string NombreProyecto { get; set; } = string.Empty;
    public string Concepto { get; set; } = string.Empty;
    public string NumeroFactura { get; set; } = string.Empty;
    public DateTime? FechaCotizacion { get; set; }
    public decimal Subtotal { get; set; }
    public decimal? Iva { get; set; }
    public decimal Total { get; set; }
    public decimal Monto { get; set; }
    public DateTime FechaPago { get; set; }
    public string MetodoPago { get; set; } = string.Empty;
    public string Referencia { get; set; } = string.Empty;
    public string Estado { get; set; } = string.Empty;
    public string ActividadStatus { get; set; } = string.Empty;
    public string Observaciones { get; set; } = string.Empty;
    public DateTime FechaRegistro { get; set; }
}

public class PagoClienteRequestDto
{
    public string Concepto { get; set; } = string.Empty;
    public string NumeroFactura { get; set; } = string.Empty;
    public DateTime? FechaCotizacion { get; set; }
    public decimal Subtotal { get; set; }
    public decimal? Iva { get; set; }
    public decimal Total { get; set; }
    public decimal Monto { get; set; }
    public DateTime FechaPago { get; set; }
    public string MetodoPago { get; set; } = string.Empty;
    public string Referencia { get; set; } = string.Empty;
    public string Estado { get; set; } = "Pendiente";
    public string ActividadStatus { get; set; } = string.Empty;
    public string Observaciones { get; set; } = string.Empty;
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
