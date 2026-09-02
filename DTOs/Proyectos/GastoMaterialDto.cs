namespace BLL_ConstruccionAPI.DTOs.Proyectos;

public class GastoMaterialDto
{
    public int Id { get; set; }
    public int ProyectoId { get; set; }
    public string Descripcion { get; set; } = string.Empty;
    public string? Caracteristicas { get; set; }
    public int? MaterialId { get; set; }
    public string Unidad { get; set; } = string.Empty;
    public decimal Cantidad { get; set; }
    public decimal CostoUnitario { get; set; }
    public decimal Total { get; set; }
    public int? ProveedorId { get; set; }
    public string? NombreProveedor { get; set; }
    public DateTime Fecha { get; set; }
    public string? Observaciones { get; set; }
    public DateTime FechaRegistro { get; set; }
}

public class GastoMaterialRequestDto
{
    public string Descripcion { get; set; } = string.Empty;
    public string? Caracteristicas { get; set; }
    public int? MaterialId { get; set; }
    public string Unidad { get; set; } = string.Empty;
    public decimal Cantidad { get; set; }
    public decimal CostoUnitario { get; set; }
    public int? ProveedorId { get; set; }
    public DateTime Fecha { get; set; } = DateTime.UtcNow;
    public string? Observaciones { get; set; }
}
