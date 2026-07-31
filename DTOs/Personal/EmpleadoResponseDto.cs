using BLL_ConstruccionAPI.Models.Personal;

namespace BLL_ConstruccionAPI.DTOs.Personal;

public class EmpleadoResponseDto
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
    public string Estatus { get; set; } = string.Empty;

    public decimal? SueldoNetoSemanal { get; set; }
    public bool CreditoInfonavit { get; set; }
    public string? TipoDescuentoInfonavit { get; set; }
    public decimal? CuotaInfonavit { get; set; }

    public string? Observaciones { get; set; }
    public DateTime FechaRegistro { get; set; }

    public string? ProyectoActualNombre { get; set; }
    public int? ProyectoActualId { get; set; }

    public static EmpleadoResponseDto FromEntity(Empleado e, string? proyectoActualNombre = null, int? proyectoActualId = null) => new()
    {
        Id                     = e.Id,
        NumeroEmpleado         = e.NumeroEmpleado,
        NombreCompleto         = e.NombreCompleto,
        Puesto                 = e.Puesto,
        CURP                   = e.CURP,
        RFC                    = e.RFC,
        NSS                    = e.NSS,
        Telefono               = e.Telefono,
        ContactoEmergencia     = e.ContactoEmergencia,
        FechaIngreso           = e.FechaIngreso,
        Estatus                = e.Estatus.ToString(),
        SueldoNetoSemanal      = e.SueldoNetoSemanal,
        CreditoInfonavit       = e.CreditoInfonavit,
        TipoDescuentoInfonavit = e.TipoDescuentoInfonavit,
        CuotaInfonavit         = e.CuotaInfonavit,
        Observaciones          = e.Observaciones,
        FechaRegistro          = e.FechaRegistro,
        ProyectoActualNombre   = proyectoActualNombre,
        ProyectoActualId       = proyectoActualId
    };
}
