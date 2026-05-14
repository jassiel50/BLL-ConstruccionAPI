namespace BLL_ConstruccionAPI.Models.Inventario.Proyectos;

public class PagoCliente
{
    public int Id { get; set; }
    public int ProyectoId { get; set; }
    public string Concepto { get; set; } = string.Empty;
    public decimal Monto { get; set; }
    public DateTime FechaPago { get; set; }
    public string MetodoPago { get; set; } = string.Empty;
    public string Referencia { get; set; } = string.Empty;
    public string Notas { get; set; } = string.Empty;
    public int RegistradoPorId { get; set; }
    public DateTime FechaRegistro { get; set; } = DateTime.UtcNow;

    public Proyecto? Proyecto { get; set; }
}
