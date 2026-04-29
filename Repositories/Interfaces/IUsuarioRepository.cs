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

    // 2FA
    Task<Usuario2FA?> Get2FAAsync(int usuarioId);
    Task Create2FAAsync(Usuario2FA usuario2FA);
    Task Update2FAAsync(Usuario2FA usuario2FA);

    // Log
    Task RegistrarLogAsync(LogAcceso log);
}