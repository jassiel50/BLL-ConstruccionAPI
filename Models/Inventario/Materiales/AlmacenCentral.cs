namespace BLL_ConstruccionAPI.Models.Inventario.Materiales
{
    public class AlmacenCentral
    {
        public int Id { get; set; }
        public int MaterialId { get; set; }
        public decimal Stock { get; set; } = 0;
        public DateTime UltimaActualizacion { get; set; } = DateTime.UtcNow;

        // Navegación
        public Material? Material { get; set; }
    }
}
