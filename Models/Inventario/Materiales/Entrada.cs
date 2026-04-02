namespace BLL_ConstruccionAPI.Models.Inventario.Materiales
{
    public class Entrada
    {
        public int Id { get; set; }
        public string NumeroFolio { get; set; } = string.Empty;
        public int ProveedorId { get; set; }
        public int UsuarioId { get; set; }          // quien registró
        public DateTime Fecha { get; set; } = DateTime.UtcNow;
        public string Observaciones { get; set; } = string.Empty;
        public decimal Total { get; set; }

        // Navegación
        public Proveedor? Proveedor { get; set; }
        public ICollection<EntradaDetalle> Detalles { get; set; } = new List<EntradaDetalle>();
    }
}
