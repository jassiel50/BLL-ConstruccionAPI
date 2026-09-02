using BLL_ConstruccionAPI.Models.Inventario.Materiales;

namespace BLL_ConstruccionAPI.Models.Inventario.Proyectos;

public class RequisicionMaterialDetalle
{
    public int Id { get; set; }
    public int RequisicionMaterialId { get; set; }
    public int Orden { get; set; }
    public string Descripcion { get; set; } = string.Empty;
    public string Unidad { get; set; } = string.Empty;
    public decimal Cantidad { get; set; }
    public string? AreaComentarios { get; set; }
    public string Status { get; set; } = "Pendiente";
    public int? MaterialId { get; set; }

    public RequisicionMaterial? RequisicionMaterial { get; set; }
    public Material? Material { get; set; }
}
