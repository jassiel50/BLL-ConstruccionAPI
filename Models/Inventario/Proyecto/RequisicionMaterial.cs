namespace BLL_ConstruccionAPI.Models.Inventario.Proyectos;

public class RequisicionMaterial
{
    public int Id { get; set; }
    public int ProyectoId { get; set; }
    public int Folio { get; set; }
    public string SeRequierePara { get; set; } = string.Empty;
    public string SeSuministraPor { get; set; } = string.Empty;
    public DateTime FechaSolicitud { get; set; } = DateTime.UtcNow;
    public string SolicitoNombre { get; set; } = string.Empty;
    public int CreadoPorId { get; set; }
    public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;

    // Fase a la que se carga el gasto extra de los renglones que suministra la empresa (no el cliente).
    public int? FaseId { get; set; }

    public Proyecto? Proyecto { get; set; }
    public FaseProyecto? Fase { get; set; }
    public List<RequisicionMaterialDetalle> Detalles { get; set; } = [];
}
