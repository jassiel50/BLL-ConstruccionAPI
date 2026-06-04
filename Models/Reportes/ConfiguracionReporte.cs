using BLL_ConstruccionAPI.Models.Auth;

namespace BLL_ConstruccionAPI.Models.Reportes;

public class ConfiguracionReporte
{
    public int Id { get; set; }
    public int UsuarioId { get; set; }
    public Usuario Usuario { get; set; } = null!;
    public string Nombre { get; set; } = string.Empty;
    public string Frecuencia { get; set; } = string.Empty; // "diario" | "semanal" | "mensual"
    public int HoraEnvio { get; set; } // 0-23
    public string Secciones { get; set; } = string.Empty; // JSON: ["inventario","proyectos",...]
    public bool Activo { get; set; } = true;
    public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;
    public DateTime? UltimoEnvio { get; set; }
}
