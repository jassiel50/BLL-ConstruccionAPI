using BLL_ConstruccionAPI.Models.Personal;

namespace BLL_ConstruccionAPI.DTOs.Personal;

public class ArchivoEmpleadoDto
{
    public int Id { get; set; }
    public int EmpleadoId { get; set; }
    public string NombreOriginal { get; set; } = string.Empty;
    public string TipoDocumento { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public long TamanioBytes { get; set; }
    public int SubidoPorId { get; set; }
    public DateTime FechaSubida { get; set; }

    public static ArchivoEmpleadoDto FromEntity(ArchivoEmpleado a) => new()
    {
        Id             = a.Id,
        EmpleadoId     = a.EmpleadoId,
        NombreOriginal = a.NombreOriginal,
        TipoDocumento  = a.TipoDocumento.ToString(),
        ContentType    = a.ContentType,
        TamanioBytes   = a.TamanioBytes,
        SubidoPorId    = a.SubidoPorId,
        FechaSubida    = a.FechaSubida
    };
}
