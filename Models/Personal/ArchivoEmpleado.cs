using BLL_ConstruccionAPI.Models.Enums;

namespace BLL_ConstruccionAPI.Models.Personal;

public class ArchivoEmpleado
{
    public int Id { get; set; }
    public int EmpleadoId { get; set; }
    public string NombreOriginal { get; set; } = string.Empty;
    public TipoDocumentoEmpleado TipoDocumento { get; set; }
    public string ContentType { get; set; } = string.Empty;
    public long TamanioBytes { get; set; }
    public byte[] Contenido { get; set; } = [];
    public int SubidoPorId { get; set; }
    public DateTime FechaSubida { get; set; } = DateTime.UtcNow;

    public Empleado? Empleado { get; set; }
}
