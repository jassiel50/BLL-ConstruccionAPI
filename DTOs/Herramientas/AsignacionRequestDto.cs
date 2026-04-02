namespace BLL_ConstruccionAPI.DTOs.Herramientas;

public class AsignacionRequestDto
{
    public int HerramientaId { get; set; }
    public int ProyectoId { get; set; }
    public int UsuarioAsignoId { get; set; }
    public int? UsuarioRecibeId { get; set; }
    public string Observaciones { get; set; } = string.Empty;
}
