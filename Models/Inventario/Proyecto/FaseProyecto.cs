using BLL_ConstruccionAPI.Models.Enums;

namespace BLL_ConstruccionAPI.Models.Inventario.Proyectos;

public class FaseProyecto
{
    public int Id { get; set; }
    public int ProyectoId { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Descripcion { get; set; } = string.Empty;
    public int Orden { get; set; }
    public DateTime FechaLimite { get; set; }
    public DateTime? FechaCompletada { get; set; }
    public EstadoFase Estado { get; set; } = EstadoFase.Pendiente;
    public DateTime FechaRegistro { get; set; } = DateTime.UtcNow;

    // Navegación
    public Proyecto? Proyecto { get; set; }
}
