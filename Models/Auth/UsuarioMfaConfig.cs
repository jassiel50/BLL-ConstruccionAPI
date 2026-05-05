namespace BLL_ConstruccionAPI.Models.Auth;

public class UsuarioMfaConfig
{
    public int Id { get; set; }
    public int UsuarioId { get; set; }
    public bool MfaHabilitado { get; set; } = false;
    public string? MfaMetodoPreferido { get; set; } // "app" | "email"
    public bool MfaEmailHabilitado { get; set; } = false;
    public bool MfaAppHabilitado { get; set; } = false;
    public DateTime? MfaUltimaActualizacion { get; set; }

    // Relación
    public Usuario? Usuario { get; set; }
}
