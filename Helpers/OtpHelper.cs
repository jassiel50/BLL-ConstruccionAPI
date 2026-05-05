using System.Security.Cryptography;
using System.Text;

namespace BLL_ConstruccionAPI.Helpers;

public static class OtpHelper
{
    /// <summary>
    /// Genera un código OTP de 6 dígitos
    /// </summary>
    public static string GenerarCodigo()
    {
        var random = RandomNumberGenerator.GetInt32(100000, 999999);
        return random.ToString();
    }

    /// <summary>
    /// Genera el hash SHA256 del código usando el email como salt
    /// </summary>
    public static string HashCodigo(string codigo, string salt)
    {
        using var sha256 = SHA256.Create();
        var saltedCode = $"{codigo}:{salt}";
        var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(saltedCode));
        return Convert.ToBase64String(bytes);
    }

    /// <summary>
    /// Verifica que un código coincida con el hash
    /// </summary>
    public static bool VerificarCodigo(string codigo, string hash, string salt)
    {
        var computedHash = HashCodigo(codigo, salt);
        return computedHash == hash;
    }

    /// <summary>
    /// Enmascara un email: ejemplo@dominio.com -> e*****@d******.com
    /// </summary>
    public static string MaskEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email) || !email.Contains('@'))
            return "***";

        var parts = email.Split('@');
        var localPart = parts[0];
        var domainParts = parts[1].Split('.');

        var maskedLocal = localPart.Length > 2
            ? $"{localPart[0]}***{localPart[^1]}"
            : $"{localPart[0]}***";

        var maskedDomain = domainParts[0].Length > 2
            ? $"{domainParts[0][0]}***"
            : "***";

        var extension = domainParts.Length > 1 ? "." + domainParts[^1] : "";

        return $"{maskedLocal}@{maskedDomain}{extension}";
    }
}
