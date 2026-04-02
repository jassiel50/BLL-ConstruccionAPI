using BLL_ConstruccionAPI.Models.Inventario.Proyectos;

namespace BLL_ConstruccionAPI.Models.Inventario.Materiales
{
    public class Salida
    {
        public int Id { get; set; }
        public string NumeroFolio { get; set; } = string.Empty;
        public int ProyectoId { get; set; }
        public int UsuarioId { get; set; }          // quien registró
        public DateTime Fecha { get; set; } = DateTime.UtcNow;
        public string Observaciones { get; set; } = string.Empty;

        // Navegación
        public Proyecto? Proyecto { get; set; }
        public ICollection<SalidaDetalle> Detalles { get; set; } = new List<SalidaDetalle>();
    }
}
