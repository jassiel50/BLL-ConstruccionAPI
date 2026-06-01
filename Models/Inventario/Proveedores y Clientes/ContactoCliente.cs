namespace BLL_ConstruccionAPI.Models.Inventario
{
    public class ContactoCliente
    {
        public int Id { get; set; }
        public int ClienteId { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string Telefono { get; set; } = string.Empty;
        public string? Email { get; set; }
        public string Cargo { get; set; } = string.Empty;
        public bool EsPrincipal { get; set; } = false;

        public Cliente? Cliente { get; set; }
    }
}
