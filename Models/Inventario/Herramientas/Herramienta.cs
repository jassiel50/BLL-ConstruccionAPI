using BLL_ConstruccionAPI.Models.Inventario.Cátalogos;

namespace BLL_ConstruccionAPI.Models.Inventario.Herramientas
{
    public class Herramienta
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
        public string Codigo { get; set; } = string.Empty;
        public string NumeroSerie { get; set; } = string.Empty;
        public int CategoriaHerramientaId { get; set; }
        public string Estado { get; set; } = "Disponible"; // Disponible, Asignada, Mantenimiento, Baja
        public decimal ValorAdquisicion { get; set; }
        public DateTime FechaAdquisicion { get; set; }
        public bool Activo { get; set; } = true;
        public DateTime FechaRegistro { get; set; } = DateTime.UtcNow;

        // Navegación
        public CategoriaHerramienta? CategoriaHerramienta { get; set; }
    }
}
