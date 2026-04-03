using BLL_ConstruccionAPI.Models.Enums;
using BLL_ConstruccionAPI.Models.Inventario;

namespace BLL_ConstruccionAPI.Models.Inventario.Proyectos
{
    public class Proyecto
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
        public string Ubicacion { get; set; } = string.Empty;
        public int ClienteId { get; set; }
        public int ResponsableId { get; set; }  // UsuarioId
        public DateTime FechaInicio { get; set; }
        public DateTime? FechaFin { get; set; }
        public EstadoProyecto Estado { get; set; } = EstadoProyecto.Activo;
        public bool Activo { get; set; } = true;
        public DateTime FechaRegistro { get; set; } = DateTime.UtcNow;

        // Navegación
        public Cliente? Cliente { get; set; }
    }
}
