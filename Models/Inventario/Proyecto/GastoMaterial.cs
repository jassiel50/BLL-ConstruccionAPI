using BLL_ConstruccionAPI.Models.Inventario;
using BLL_ConstruccionAPI.Models.Inventario.Materiales;

namespace BLL_ConstruccionAPI.Models.Inventario.Proyectos;

/// <summary>
/// Registro manual de una compra de material a cargo de la empresa (no del cliente) en un
/// proyecto. Es la fuente del "Gasto de Materiales" del panel financiero, en un apartado
/// propio separado de Gastos Extras.
/// </summary>
public class GastoMaterial
{
    public int Id { get; set; }
    public int ProyectoId { get; set; }
    public string Descripcion { get; set; } = string.Empty;
    public string? Caracteristicas { get; set; }
    public int? MaterialId { get; set; }
    public string Unidad { get; set; } = string.Empty;
    public decimal Cantidad { get; set; }
    public decimal CostoUnitario { get; set; }
    public int? ProveedorId { get; set; }
    public DateTime Fecha { get; set; } = DateTime.UtcNow;
    public string? Observaciones { get; set; }
    public int CreadoPorId { get; set; }
    public DateTime FechaRegistro { get; set; } = DateTime.UtcNow;

    public Proyecto? Proyecto { get; set; }
    public Material? Material { get; set; }
    public Proveedor? Proveedor { get; set; }
}
