using BLL_ConstruccionAPI.Models.Enums;

namespace BLL_ConstruccionAPI.Models.Inventario.Proyectos;

public class ArchivoProyecto
{
    public int Id { get; set; }
    public int ProyectoId { get; set; }
    public string NombreOriginal { get; set; } = string.Empty;
    public TipoDocumentoProyecto TipoDocumento { get; set; }
    public string ContentType { get; set; } = string.Empty;
    public long TamanioBytes { get; set; }
    public byte[] Contenido { get; set; } = [];
    public int SubidoPorId { get; set; }
    public DateTime FechaSubida { get; set; } = DateTime.UtcNow;

    public Proyecto? Proyecto { get; set; }
}
