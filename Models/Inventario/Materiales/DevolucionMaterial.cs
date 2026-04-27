using BLL_ConstruccionAPI.Models.Inventario.Proyectos;

namespace BLL_ConstruccionAPI.Models.Inventario.Materiales;

public class DevolucionMaterial
{
    public int Id { get; set; }
    public int ProyectoId { get; set; }
    public int MaterialId { get; set; }
    public int UsuarioId { get; set; }
    public decimal CantidadDevuelta { get; set; }
    public string Observaciones { get; set; } = string.Empty;
    public DateTime FechaDevolucion { get; set; } = DateTime.UtcNow;

    // Navegación
    public Proyecto? Proyecto { get; set; }
    public Material? Material { get; set; }
}
