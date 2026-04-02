namespace BLL_ConstruccionAPI.DTOs.Auth
{
    public class Enable2FAResponseDto
    {
        public string SecretKey { get; set; } = string.Empty;
        public string QrCodeUrl { get; set; } = string.Empty;
    }
}
