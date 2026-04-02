namespace BLL_ConstruccionAPI.DTOs.Salidas;

public class SalidaRequestDto
{
    public string NumeroFolio { get; set; } = string.Empty;
    public int ProyectoId { get; set; }
    public int UsuarioId { get; set; }
    public string Observaciones { get; set; } = string.Empty;
    public List<SalidaDetalleDto> Detalles { get; set; } = [];
}
