namespace BLL_ConstruccionAPI.DTOs.Personal;

public class EmpleadoRequestDto
{
    public string NumeroEmpleado { get; set; } = string.Empty;
    public string NombreCompleto { get; set; } = string.Empty;
    public string Puesto { get; set; } = string.Empty;

    public string? CURP { get; set; }
    public string? RFC { get; set; }
    public string? NSS { get; set; }

    public string? Telefono { get; set; }
    public string? ContactoEmergencia { get; set; }

    public DateTime? FechaIngreso { get; set; }

    public decimal? SueldoNetoSemanal { get; set; }
    public bool CreditoInfonavit { get; set; }
    public string? TipoDescuentoInfonavit { get; set; }
    public decimal? CuotaInfonavit { get; set; }

    public string? Observaciones { get; set; }
}
