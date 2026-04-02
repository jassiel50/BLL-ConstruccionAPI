namespace BLL_ConstruccionAPI.Models.Inventario.Cátalogos
{
    public class UnidadMedida
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;      // Kilogramo
        public string Abreviatura { get; set; } = string.Empty; // kg
        public bool Activo { get; set; } = true;
    }
}
