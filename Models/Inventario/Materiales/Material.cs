using BLL_ConstruccionAPI.Models.Enums;
using BLL_ConstruccionAPI.Models.Inventario.Cátalogos;

namespace BLL_ConstruccionAPI.Models.Inventario.Materiales
{
    public class Material
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
        public string Codigo { get; set; } = string.Empty;
        public int CategoriaId { get; set; }
        public int UnidadMedidaId { get; set; }
        public decimal StockMinimo { get; set; }
        public decimal PrecioUnitario { get; set; }
        public Zona Zona { get; set; } = Zona.Guadalajara;
        public TipoUbicacion TipoUbicacion { get; set; } = TipoUbicacion.Almacen;
        public bool EnProyecto { get; set; } = false;
        public bool Activo { get; set; } = true;
        public DateTime FechaRegistro { get; set; } = DateTime.UtcNow;

        // Navegación
        public CategoriaMaterial? Categoria { get; set; }
        public UnidadMedida? UnidadMedida { get; set; }
    }
}
