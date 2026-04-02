using BLL_ConstruccionAPI.Models.Inventario.Proyectos;

namespace BLL_ConstruccionAPI.Models.Inventario.Herramientas
{
    public class AsignacionHerramienta
    {
        public int Id { get; set; }
        public int HerramientaId { get; set; }
        public int ProyectoId { get; set; }
        public int UsuarioAsignoId { get; set; }     // quien la asignó
        public int? UsuarioRecibeId { get; set; }    // quien la recibió en el proyecto
        public DateTime FechaAsignacion { get; set; } = DateTime.UtcNow;
        public DateTime? FechaDevolucion { get; set; }
        public string Estado { get; set; } = "Asignada"; // Asignada, Devuelta
        public string Observaciones { get; set; } = string.Empty;
        public string? ObservacionesDevolucion { get; set; }

        // Navegación
        public Herramienta? Herramienta { get; set; }
        public Proyecto? Proyecto { get; set; }
    }
}
