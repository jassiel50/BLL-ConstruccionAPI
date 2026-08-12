using BLL_ConstruccionAPI.Models.Servicios;

namespace BLL_ConstruccionAPI.DTOs.Servicios;

// Vista recortada de un Servicio para el flujo sin sesión (acceso por liga/token):
// no expone OperadorId, ClienteId interno ni otros datos que no necesita quien llena la liga.
public class ServicioPublicoDto
{
    public int Id { get; set; }

    public string ClienteNombre { get; set; } = string.Empty;
    public string ClienteDireccion { get; set; } = string.Empty;
    public string ClienteTelefono { get; set; } = string.Empty;

    public string Tipo { get; set; } = string.Empty;
    public string Equipo { get; set; } = string.Empty;
    public string DireccionServicio { get; set; } = string.Empty;
    public string DescripcionTrabajo { get; set; } = string.Empty;
    public string MaterialesUtilizados { get; set; } = string.Empty;
    public string Observaciones { get; set; } = string.Empty;

    public string Estado { get; set; } = string.Empty;

    public string? OperadorNombre { get; set; }

    public string? NombreSolicitante { get; set; }
    public string? FirmaSolicitanteBase64 { get; set; }
    public DateTime? FechaFirmaSolicitante { get; set; }

    public string? HorarioTrabajo { get; set; }
    public int? NumeroTrabajadores { get; set; }
    public decimal? TotalHorasTrabajadas { get; set; }

    public string? RecursoManoDeObra { get; set; }
    public string? RecursoHerramienta { get; set; }
    public string? RecursoRefacciones { get; set; }
    public string? RecursoConsumibles { get; set; }

    public List<string> TiposTrabajo { get; set; } = [];
    public List<string> EquipoTrabajo { get; set; } = [];

    public DateTime TokenExpira { get; set; }

    public static ServicioPublicoDto FromEntity(Servicio s) => new()
    {
        Id                   = s.Id,
        ClienteNombre        = s.ClienteId.HasValue && s.Cliente is not null ? s.Cliente.Nombre : s.ClienteNombre,
        ClienteDireccion     = s.ClienteId.HasValue && s.Cliente is not null ? s.Cliente.Direccion : s.ClienteDireccion,
        ClienteTelefono      = s.ClienteTelefono,
        Tipo                 = s.Tipo?.ToString() ?? "",
        Equipo               = s.Equipo,
        DireccionServicio    = s.DireccionServicio,
        DescripcionTrabajo   = s.DescripcionTrabajo,
        MaterialesUtilizados = s.MaterialesUtilizados,
        Observaciones        = s.Observaciones,
        Estado               = s.Estado.ToString(),
        OperadorNombre         = s.OperadorNombre,
        NombreSolicitante      = s.NombreSolicitante,
        FirmaSolicitanteBase64 = s.FirmaSolicitanteBase64,
        FechaFirmaSolicitante  = s.FechaFirmaSolicitante,
        HorarioTrabajo         = s.HorarioTrabajo,
        NumeroTrabajadores     = s.NumeroTrabajadores,
        TotalHorasTrabajadas   = s.TotalHorasTrabajadas,
        RecursoManoDeObra      = s.RecursoManoDeObra,
        RecursoHerramienta     = s.RecursoHerramienta,
        RecursoRefacciones     = s.RecursoRefacciones,
        RecursoConsumibles     = s.RecursoConsumibles,
        TiposTrabajo           = string.IsNullOrWhiteSpace(s.TiposTrabajo)
            ? []
            : s.TiposTrabajo.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).ToList(),
        EquipoTrabajo          = string.IsNullOrWhiteSpace(s.EquipoTrabajo)
            ? []
            : s.EquipoTrabajo.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).ToList(),
        TokenExpira          = s.TokenExpira ?? DateTime.UtcNow
    };
}
