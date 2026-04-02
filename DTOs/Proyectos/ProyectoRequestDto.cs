namespace BLL_ConstruccionAPI.DTOs.Proyectos;

public class ProyectoRequestDto
{
    public string Nombre { get; set; } = string.Empty;
    public string Descripcion { get; set; } = string.Empty;
    public string Ubicacion { get; set; } = string.Empty;
    public int ClienteId { get; set; }
    public int ResponsableId { get; set; }
    public DateTime FechaInicio { get; set; }
    public DateTime? FechaFin { get; set; }
    public string Estado { get; set; } = "Activo";
}
