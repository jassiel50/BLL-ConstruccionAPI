namespace BLL_ConstruccionAPI.Models.Inventario.Materiales
{
    public class SalidaDetalle
    {
        public int Id { get; set; }
        public int SalidaId { get; set; }
        public int MaterialId { get; set; }
        public decimal Cantidad { get; set; }

        // Navegación
        public Material? Material { get; set; }
    }
}
