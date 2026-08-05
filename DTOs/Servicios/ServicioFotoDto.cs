using BLL_ConstruccionAPI.Models.Servicios;

namespace BLL_ConstruccionAPI.DTOs.Servicios;

public class ServicioFotoDto
{
    public int Id { get; set; }
    public int ServicioId { get; set; }
    public string NombreOriginal { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public long TamanioBytes { get; set; }
    public DateTime FechaCaptura { get; set; }

    public static ServicioFotoDto FromEntity(ServicioFoto f) => new()
    {
        Id             = f.Id,
        ServicioId     = f.ServicioId,
        NombreOriginal = f.NombreOriginal,
        ContentType    = f.ContentType,
        TamanioBytes   = f.TamanioBytes,
        FechaCaptura   = f.FechaCaptura
    };
}
