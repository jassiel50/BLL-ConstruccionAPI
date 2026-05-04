using BLL_ConstruccionAPI.Models.Inventario.Proyectos;

namespace BLL_ConstruccionAPI.DTOs.Proyectos;

public class ProyectoResponseDto
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Descripcion { get; set; } = string.Empty;
    public string Ubicacion { get; set; } = string.Empty;
    public int ClienteId { get; set; }
    public string NombreCliente { get; set; } = string.Empty;
    public int ResponsableId { get; set; }
    public DateTime FechaInicio { get; set; }
    public DateTime? FechaFin { get; set; }
    public string Estado { get; set; } = string.Empty;
    public DateTime FechaRegistro { get; set; }
    public string NumeroCotizacion { get; set; } = string.Empty;
    public string OrdenCompra { get; set; } = string.Empty;
    public decimal MontoContrato { get; set; } = 0;
    public decimal PresupuestoEstimado { get; set; } = 0;
    public decimal GastoMateriales { get; set; } = 0;
    public decimal GastoHerramientas { get; set; } = 0;
    public decimal GastoExtras { get; set; } = 0;
    public decimal GastoReal { get; set; } = 0;
    public decimal Utilidad { get; set; } = 0;
    public decimal Varianza { get; set; } = 0;
    public bool SobrepasadoPresupuesto { get; set; }
    public bool SobrepasadoContrato { get; set; }

    public static ProyectoResponseDto FromEntity(Proyecto e) => new()
    {
        Id = e.Id,
        Nombre = e.Nombre,
        Descripcion = e.Descripcion,
        Ubicacion = e.Ubicacion,
        ClienteId = e.ClienteId,
        NombreCliente = e.Cliente?.Nombre ?? string.Empty,
        ResponsableId = e.ResponsableId,
        FechaInicio = e.FechaInicio,
        FechaFin = e.FechaFin,
        Estado = e.Estado.ToString(),
        FechaRegistro = e.FechaRegistro,
        NumeroCotizacion = e.NumeroCotizacion,
        OrdenCompra = e.OrdenCompra,
        MontoContrato = e.MontoContrato,
        PresupuestoEstimado = e.PresupuestoEstimado
    };
}
