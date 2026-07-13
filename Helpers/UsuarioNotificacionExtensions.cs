using BLL_ConstruccionAPI.Models.Auth;

namespace BLL_ConstruccionAPI.Helpers;

public static class UsuarioNotificacionExtensions
{
    /// <summary>
    /// Correos a los que deben enviarse notificaciones/reportes automáticos para este usuario:
    /// su correo principal y, si tiene uno registrado, su correo secundario.
    /// No usar para códigos de seguridad (MFA, restablecimiento de contraseña) — esos solo van al correo principal.
    /// </summary>
    public static IEnumerable<string> CorreosNotificacion(this Usuario usuario)
    {
        yield return usuario.Email;

        if (!string.IsNullOrWhiteSpace(usuario.EmailSecundario) &&
            !usuario.EmailSecundario.Equals(usuario.Email, StringComparison.OrdinalIgnoreCase))
        {
            yield return usuario.EmailSecundario;
        }
    }
}
