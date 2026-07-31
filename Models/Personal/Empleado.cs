using BLL_ConstruccionAPI.Models.Enums;

namespace BLL_ConstruccionAPI.Models.Personal;

public class Empleado
{
    public int Id { get; set; }

    public string NumeroEmpleado { get; set; } = string.Empty;
    public string NombreCompleto { get; set; } = string.Empty;
    public string Puesto { get; set; } = string.Empty;

    public string? CURP { get; set; }
    public string? RFC { get; set; }
    public string? NSS { get; set; }

    public string? Telefono { get; set; }
    public string? ContactoEmergencia { get; set; }

    public DateTime? FechaIngreso { get; set; }
    public EstatusEmpleado Estatus { get; set; } = EstatusEmpleado.Activo;

    // Campos listos para la fase de Nómina — se capturan pero no se usan todavía
    public decimal? SueldoNetoSemanal { get; set; }
    public bool CreditoInfonavit { get; set; }
    public string? TipoDescuentoInfonavit { get; set; }
    public decimal? CuotaInfonavit { get; set; }

    public string? Observaciones { get; set; }
    public DateTime FechaRegistro { get; set; } = DateTime.UtcNow;

    // Navegación
    public ICollection<ArchivoEmpleado> Archivos { get; set; } = new List<ArchivoEmpleado>();
    public ICollection<AsignacionEmpleadoProyecto> Asignaciones { get; set; } = new List<AsignacionEmpleadoProyecto>();
}
