namespace BLL_ConstruccionAPI.DTOs.Auth;

public class ChangeMfaMethodRequestDto
{
    public string PasswordActual { get; set; } = string.Empty;
    public string MetodoActualCodigo { get; set; } = string.Empty;
    public string MetodoNuevo { get; set; } = string.Empty; // "app" | "email"
}
