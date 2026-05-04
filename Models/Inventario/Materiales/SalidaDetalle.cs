namespace BLL_ConstruccionAPI.Models.Inventario.Materiales
{
    public class SalidaDetalle
    {
        public int Id { get; set; }
        public int SalidaId { get; set; }
        public int MaterialId { get; set; }
        public decimal Cantidad { get; set; }
        public decimal PrecioUnitario { get; set; } = 0;

        // Navegación
        public Material? Material { get; set; }
    }
}
