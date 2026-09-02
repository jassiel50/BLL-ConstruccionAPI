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

    // Quién surte este renglón: "Cliente" (default, la mayoría) o "Empresa" (BLL lo compra/pone).
    public string Responsable { get; set; } = "Cliente";
    // Costo unitario cuando Responsable = "Empresa"; genera un GastoExtra por Cantidad * CostoUnitario.
    public decimal CostoUnitario { get; set; } = 0;
    // Referencia al GastoExtra generado automáticamente para este renglón (si aplica).
    public int? GastoExtraId { get; set; }

    public RequisicionMaterial? RequisicionMaterial { get; set; }
    public Material? Material { get; set; }
    public GastoExtra? GastoExtra { get; set; }
}
