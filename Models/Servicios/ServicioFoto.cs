namespace BLL_ConstruccionAPI.Models.Servicios;

public class ServicioFoto
{
    public int Id { get; set; }
    public int ServicioId { get; set; }

    public string NombreOriginal { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public long TamanioBytes { get; set; }
    public byte[] Contenido { get; set; } = [];

    public DateTime FechaCaptura { get; set; } = DateTime.UtcNow;

    public Servicio? Servicio { get; set; }
}
