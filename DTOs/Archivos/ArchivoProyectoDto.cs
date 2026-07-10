namespace BLL_ConstruccionAPI.DTOs.Archivos;

public class ArchivoProyectoDto
{
    public int Id { get; set; }
    public int ProyectoId { get; set; }
    public string NombreOriginal { get; set; } = string.Empty;
    public string TipoDocumento { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public long TamanioBytes { get; set; }
    public int SubidoPorId { get; set; }
    public DateTime FechaSubida { get; set; }
    public int? CarpetaId { get; set; }
    public string? CarpetaNombre { get; set; }
}
