namespace BLL_ConstruccionAPI.Models.Reportes;

public class ReporteEntregaConformidadFoto
{
    public int Id { get; set; }
    public int ReporteEntregaConformidadId { get; set; }

    public string NombreOriginal { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public long TamanioBytes { get; set; }
    public byte[] Contenido { get; set; } = [];

    public DateTime FechaCaptura { get; set; } = DateTime.UtcNow;

    public ReporteEntregaConformidad? ReporteEntregaConformidad { get; set; }
}
