namespace BLL_ConstruccionAPI.DTOs.Entradas;

public class EntradaRequestDto
{
    public string NumeroFolio { get; set; } = string.Empty;
    public int ProveedorId { get; set; }
    public int UsuarioId { get; set; }
    public string Observaciones { get; set; } = string.Empty;
    public List<EntradaDetalleDto> Detalles { get; set; } = [];
}
