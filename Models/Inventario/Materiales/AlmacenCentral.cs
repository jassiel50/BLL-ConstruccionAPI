using BLL_ConstruccionAPI.Models.Enums;

namespace BLL_ConstruccionAPI.Models.Inventario.Materiales
{
    public class AlmacenCentral
    {
        public int Id { get; set; }
        public int MaterialId { get; set; }
        public decimal Stock { get; set; } = 0;
        public Zona Zona { get; set; } = Zona.Guadalajara;
        public TipoUbicacion TipoUbicacion { get; set; } = TipoUbicacion.Almacen;
        public DateTime UltimaActualizacion { get; set; } = DateTime.UtcNow;

        // Navegación
        public Material? Material { get; set; }
    }
}
