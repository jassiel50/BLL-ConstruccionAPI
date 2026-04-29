using BLL_ConstruccionAPI.Data;
using BLL_ConstruccionAPI.Models.Auth;
using BLL_ConstruccionAPI.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace BLL_ConstruccionAPI.Repositories;

public class UsuarioRepository : IUsuarioRepository
{
    private readonly AppDbContext _context;

    public UsuarioRepository(AppDbContext context)
    {
        _context = context;
    }

    // Usuarios
    public async Task<Usuario?> GetByIdAsync(int id)
        => await _context.Usuarios.FindAsync(id);

    public async Task<Usuario?> GetByNombreUsuarioAsync(string nombreUsuario)
        => await _context.Usuarios.FirstOrDefaultAsync(u => u.NombreUsuario == nombreUsuario);

    public async Task<Usuario?> GetByEmailAsync(string email)
        => await _context.Usuarios.FirstOrDefaultAsync(u => u.Email == email);

    public async Task<bool> ExisteNombreUsuarioAsync(string nombreUsuario)
        => await _context.Usuarios.AnyAsync(u => u.NombreUsuario == nombreUsuario);

    public async Task<bool> ExisteEmailAsync(string email)
        => await _context.Usuarios.AnyAsync(u => u.Email == email);

    public async Task<int> CreateAsync(Usuario usuario)
    {
        _context.Usuarios.Add(usuario);
        await _context.SaveChangesAsync();
        return usuario.Id;
    }

    public async Task UpdateAsync(Usuario usuario)
    {
        _context.Usuarios.Update(usuario);
        await _context.SaveChangesAsync();
    }

    // Tokens
    public async Task<TokenSesion?> GetTokenAsync(string token)
        => await _context.TokensSesion.FirstOrDefaultAsync(t => t.Token == token && !t.Revocado);

    public async Task CreateTokenAsync(TokenSesion token)
    {
        _context.TokensSesion.Add(token);
        await _context.SaveChangesAsync();
    }

    public async Task RevocarTokenAsync(string token)
    {
        var tokenSesion = await _context.TokensSesion.FirstOrDefaultAsync(t => t.Token == token);
        if (tokenSesion is not null)
        {
            tokenSesion.Revocado = true;
            await _context.SaveChangesAsync();
        }
    }

    public async Task RevocarTodosLosTokensAsync(int usuarioId)
    {
        var tokens = await _context.TokensSesion
            .Where(t => t.UsuarioId == usuarioId && !t.Revocado)
            .ToListAsync();

        tokens.ForEach(t => t.Revocado = true);
        await _context.SaveChangesAsync();
    }

    // 2FA
    public async Task<Usuario2FA?> Get2FAAsync(int usuarioId)
        => await _context.Usuarios2FA.FirstOrDefaultAsync(u => u.UsuarioId == usuarioId);

    public async Task Create2FAAsync(Usuario2FA usuario2FA)
    {
        _context.Usuarios2FA.Add(usuario2FA);
        await _context.SaveChangesAsync();
    }

    public async Task Update2FAAsync(Usuario2FA usuario2FA)
    {
        _context.Usuarios2FA.Update(usuario2FA);
        await _context.SaveChangesAsync();
    }

    // Log
    public async Task RegistrarLogAsync(LogAcceso log)
    {
        _context.LogAccesos.Add(log);
        await _context.SaveChangesAsync();
    }

    public async Task<IEnumerable<Usuario>> GetAllAsync()
        => await _context.Usuarios
            .Include(u => u.Rol)
            .OrderByDescending(u => u.FechaCreacion)
            .ToListAsync();

    public async Task<Usuario?> GetByIdConRolAsync(int id)
        => await _context.Usuarios
            .Include(u => u.Rol)
            .FirstOrDefaultAsync(u => u.Id == id);
}