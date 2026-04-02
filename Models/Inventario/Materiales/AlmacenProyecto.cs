using BLL_ConstruccionAPI.Models.Inventario.Proyectos;

namespace BLL_ConstruccionAPI.Models.Inventario.Materiales
{
    public class AlmacenProyecto
    {
        public int Id { get; set; }
        public int ProyectoId { get; set; }
        public int MaterialId { get; set; }
        public decimal Stock { get; set; } = 0;
        public DateTime UltimaActualizacion { get; set; } = DateTime.UtcNow;

        // Navegación
        public Proyecto? Proyecto { get; set; }
        public Material? Material { get; set; }
    }
}
