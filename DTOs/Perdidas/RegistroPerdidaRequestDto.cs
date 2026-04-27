namespace BLL_ConstruccionAPI.DTOs.Perdidas;

public class RegistroPerdidaRequestDto
{
    public string Tipo { get; set; } = string.Empty;        // "Material" o "Herramienta"
    public int? MaterialId { get; set; }
    public int? HerramientaId { get; set; }
    public int? ProyectoId { get; set; }
    public string Motivo { get; set; } = string.Empty;
    public decimal CantidadPerdida { get; set; }
    public string Descripcion { get; set; } = string.Empty;
    public DateTime FechaPerdida { get; set; }
}
