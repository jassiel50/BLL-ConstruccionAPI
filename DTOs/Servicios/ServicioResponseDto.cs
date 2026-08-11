using BLL_ConstruccionAPI.Models.Servicios;

namespace BLL_ConstruccionAPI.DTOs.Servicios;

public class ServicioResponseDto
{
    public int Id { get; set; }

    public int? ClienteId { get; set; }
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

    public int OperadorId { get; set; }
    public string OperadorNombre { get; set; } = string.Empty;

    public DateTime FechaInicio { get; set; }
    public DateTime? FechaFin { get; set; }

    public string? FirmaBase64 { get; set; }
    public string? NombreQuienFirma { get; set; }
    public DateTime? FechaFirma { get; set; }

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

    public DateTime FechaCreacion { get; set; }

    public static ServicioResponseDto FromEntity(Servicio s) => new()
    {
        Id                   = s.Id,
        ClienteId            = s.ClienteId,
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
        OperadorId           = s.OperadorId,
        OperadorNombre       = s.OperadorNombre,
        FechaInicio          = s.FechaInicio,
        FechaFin             = s.FechaFin,
        FirmaBase64          = s.FirmaBase64,
        NombreQuienFirma     = s.NombreQuienFirma,
        FechaFirma           = s.FechaFirma,
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
        FechaCreacion        = s.FechaCreacion
    };
}
