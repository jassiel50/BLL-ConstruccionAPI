namespace BLL_ConstruccionAPI.DTOs.Nomina;

public class GenerarPeriodoNominaRequestDto
{
    public DateTime FechaInicio { get; set; }
    public DateTime FechaFin { get; set; }
}

public class NominaDetalleDto
{
    public int Id { get; set; }
    public int EmpleadoId { get; set; }
    public string EmpleadoNombre { get; set; } = string.Empty;
    public string EmpleadoNumero { get; set; } = string.Empty;
    public int? ProyectoId { get; set; }
    public string? ProyectoNombre { get; set; }

    public decimal SueldoBruto { get; set; }
    public decimal DescuentoInfonavit { get; set; }
    public decimal SueldoNeto { get; set; }

    public bool Pagado { get; set; }
    public DateTime? FechaPago { get; set; }
}

public class PeriodoNominaDto
{
    public int Id { get; set; }
    public DateTime FechaInicio { get; set; }
    public DateTime FechaFin { get; set; }
    public DateTime FechaGeneracion { get; set; }
    public string Estado { get; set; } = string.Empty;

    public int TotalEmpleados { get; set; }
    public decimal TotalBruto { get; set; }
    public decimal TotalDescuentos { get; set; }
    public decimal TotalNeto { get; set; }

    public List<NominaDetalleDto> Detalles { get; set; } = [];
}

public class HistorialNominaEmpleadoDto
{
    public int PeriodoNominaId { get; set; }
    public DateTime FechaInicio { get; set; }
    public DateTime FechaFin { get; set; }
    public string? ProyectoNombre { get; set; }
    public decimal SueldoBruto { get; set; }
    public decimal DescuentoInfonavit { get; set; }
    public decimal SueldoNeto { get; set; }
    public bool Pagado { get; set; }
    public DateTime? FechaPago { get; set; }
}

public class CostoProyectoNominaDto
{
    public int ProyectoId { get; set; }
    public string ProyectoNombre { get; set; } = string.Empty;
    public decimal TotalManoDeObra { get; set; }
    public int TotalRegistros { get; set; }
}
