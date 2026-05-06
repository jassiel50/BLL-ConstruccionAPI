using BLL_ConstruccionAPI.Models.Auth;

namespace BLL_ConstruccionAPI.Repositories.Interfaces;

public interface IUsuarioRepository
{
    Task<Usuario?> GetByIdAsync(int id);
    Task<Usuario?> GetByNombreUsuarioAsync(string nombreUsuario);
    Task<Usuario?> GetByEmailAsync(string email);
    Task<bool> ExisteNombreUsuarioAsync(string nombreUsuario);
    Task<bool> ExisteEmailAsync(string email);
    Task<int> CreateAsync(Usuario usuario);
    Task UpdateAsync(Usuario usuario);
    Task<IEnumerable<Usuario>> GetAllAsync();
    Task<Usuario?> GetByIdConRolAsync(int id);

    // Tokens
    Task<TokenSesion?> GetTokenAsync(string token);
    Task CreateTokenAsync(TokenSesion token);
    Task RevocarTokenAsync(string token);
    Task RevocarTodosLosTokensAsync(int usuarioId);

    // 2FA (Legacy - mantener compatibilidad)
    Task<Usuario2FA?> Get2FAAsync(int usuarioId);
    Task Create2FAAsync(Usuario2FA usuario2FA);
    Task Update2FAAsync(Usuario2FA usuario2FA);

    // MFA Config
    Task<UsuarioMfaConfig?> GetMfaConfigAsync(int usuarioId);
    Task CreateMfaConfigAsync(UsuarioMfaConfig config);
    Task UpdateMfaConfigAsync(UsuarioMfaConfig config);

    // MFA Email Codes
    Task CreateMfaEmailCodeAsync(MfaEmailCode code);
    Task<MfaEmailCode?> GetMfaEmailCodeValidoAsync(int usuarioId);
    Task InvalidarCodigosAnterioresAsync(int usuarioId, string canal);
    Task RegistrarIntentoFallidoAsync(int codeId);
    Task MarcarMfaEmailCodigoUsadoAsync(MfaEmailCode code);

    // Password Reset Codes
    Task CreatePasswordResetCodeAsync(PasswordResetCode code);
    Task<PasswordResetCode?> GetPasswordResetCodeValidoAsync(int usuarioId);
    Task InvalidarPasswordResetCodigosAsync(int usuarioId);
    Task RegistrarIntentoFallidoPasswordResetAsync(int codeId);

    // Log
    Task RegistrarLogAsync(LogAcceso log);

    // Notificaciones
    Task<List<Usuario>> GetUsuariosNotificablesAsync();
}