using BLL_ConstruccionAPI.Models.Reportes;
using System.Text.Json;

namespace BLL_ConstruccionAPI.DTOs.Reportes;

public class ConfiguracionReporteResponseDto
{
    public int Id { get; set; }
    public int UsuarioId { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Frecuencia { get; set; } = string.Empty;
    public int HoraEnvio { get; set; }
    public List<string> Secciones { get; set; } = [];
    public bool Activo { get; set; }
    public DateTime FechaCreacion { get; set; }
    public DateTime? UltimoEnvio { get; set; }

    public static ConfiguracionReporteResponseDto FromEntity(ConfiguracionReporte e) => new()
    {
        Id = e.Id,
        UsuarioId = e.UsuarioId,
        Nombre = e.Nombre,
        Frecuencia = e.Frecuencia,
        HoraEnvio = e.HoraEnvio,
        Secciones = JsonSerializer.Deserialize<List<string>>(e.Secciones) ?? [],
        Activo = e.Activo,
        FechaCreacion = e.FechaCreacion,
        UltimoEnvio = e.UltimoEnvio
    };
}
