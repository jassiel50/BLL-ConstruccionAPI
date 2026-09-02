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

    public Proyecto? Proyecto { get; set; }
    public List<RequisicionMaterialDetalle> Detalles { get; set; } = [];
}
