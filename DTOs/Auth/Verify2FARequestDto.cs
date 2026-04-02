namespace BLL_ConstruccionAPI.DTOs.Auth
{
    public class Verify2FARequestDto
    {
        public int UsuarioId { get; set; }
        public string Codigo { get; set; } = string.Empty;
    }
}
