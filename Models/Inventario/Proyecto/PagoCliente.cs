using BLL_ConstruccionAPI.Models.Enums;

namespace BLL_ConstruccionAPI.Models.Inventario.Proyectos;

public class PagoCliente
{
    public int Id { get; set; }
    public int ProyectoId { get; set; }
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
    public EstadoPago Estado { get; set; } = EstadoPago.Pendiente;
    public string ActividadStatus { get; set; } = string.Empty;
    public string Observaciones { get; set; } = string.Empty;
    public int RegistradoPorId { get; set; }
    public DateTime FechaRegistro { get; set; } = DateTime.UtcNow;

    public Proyecto? Proyecto { get; set; }
}
