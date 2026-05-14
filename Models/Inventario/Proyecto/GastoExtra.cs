using BLL_ConstruccionAPI.Models.Inventario;

namespace BLL_ConstruccionAPI.Models.Inventario.Proyectos;

public class GastoExtra
{
    public int Id { get; set; }
    public int FaseId { get; set; }
    public string Concepto { get; set; } = string.Empty;
    public decimal Monto { get; set; }
    public decimal MontoProveedor { get; set; } = 0;
    public decimal CobradoCliente { get; set; } = 0;
    public int? ProveedorId { get; set; }
    public DateTime Fecha { get; set; } = DateTime.UtcNow;
    public string Observaciones { get; set; } = string.Empty;
    public DateTime FechaRegistro { get; set; } = DateTime.UtcNow;

    public FaseProyecto? Fase { get; set; }
    public Proveedor? Proveedor { get; set; }
}
