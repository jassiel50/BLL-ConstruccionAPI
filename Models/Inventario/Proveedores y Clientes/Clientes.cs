namespace BLL_ConstruccionAPI.Models.Inventario
{
    public class Cliente
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string RFC { get; set; } = string.Empty;
        public string Direccion { get; set; } = string.Empty;
        public string Estado { get; set; } = string.Empty;
        public bool Activo { get; set; } = true;
        public DateTime FechaRegistro { get; set; } = DateTime.UtcNow;

        public ICollection<ContactoCliente> Contactos { get; set; } = new List<ContactoCliente>();
    }
}
