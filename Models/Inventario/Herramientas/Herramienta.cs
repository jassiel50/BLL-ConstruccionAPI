using BLL_ConstruccionAPI.Models.Enums;
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
        public Zona Zona { get; set; } = Zona.Guadalajara;

        /// <summary>Ubicación física libre (ej. "Planta Texcoco", "Almacén Central"). Ya no es un enum fijo.</summary>
        public string TipoUbicacion { get; set; } = string.Empty;
        public EstadoHerramienta Estado { get; set; } = EstadoHerramienta.Disponible;
        public decimal ValorAdquisicion { get; set; }
        public DateTime FechaAdquisicion { get; set; }
        public int Cantidad { get; set; } = 1;
        public bool Activo { get; set; } = true;
        public DateTime FechaRegistro { get; set; } = DateTime.UtcNow;

        // Navegación
        public CategoriaHerramienta? CategoriaHerramienta { get; set; }
    }
}
