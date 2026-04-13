using BLL_ConstruccionAPI.Models.Enums;

namespace BLL_ConstruccionAPI.Models.Inventario.Materiales
{
    public class EntradaDetalle
    {
        public int Id { get; set; }
        public int EntradaId { get; set; }
        public int MaterialId { get; set; }
        public decimal Cantidad { get; set; }
        public decimal PrecioUnitario { get; set; }
        public decimal Subtotal { get; set; }
        public Zona Zona { get; set; }
        public TipoUbicacion TipoUbicacion { get; set; }

        // Navegación
        public Material? Material { get; set; }
    }
}
