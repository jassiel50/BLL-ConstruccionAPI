namespace BLL_ConstruccionAPI.DTOs.Nomina;

public class GenerarPeriodoNominaRequestDto
{
    public DateTime FechaInicio { get; set; }
    public DateTime FechaFin { get; set; }
    public List<AjusteEmpleadoDto> Ajustes { get; set; } = [];
    public List<AsistenciaEmpleadoDto> Asistencias { get; set; } = [];
}

// Asistencia por empleado dentro del rango del periodo. Solo se envían los días
// que difieren del default (Estado != Presente o HorasExtra > 0); un día ausente
// del arreglo se interpreta como Presente, sin horas extra.
public class AsistenciaEmpleadoDto
{
    public int EmpleadoId { get; set; }
    public List<AsistenciaDiaDto> Dias { get; set; } = [];
}

public class AsistenciaDiaDto
{
    public DateTime Fecha { get; set; }
    public string Estado { get; set; } = "Presente";
    public decimal HorasExtra { get; set; }
}

// Ajuste manual por empleado al generar un periodo: positivo (bono, hora extra)
// o negativo (descuento por falta, retardo, etc.).
public class AjusteEmpleadoDto
{
    public int EmpleadoId { get; set; }
    public decimal MontoAjuste { get; set; }
    public string? MotivoAjuste { get; set; }

    // true = no se le genera pago en este periodo (p.ej. personal freelance que no trabajó esta semana).
    public bool Excluido { get; set; }
}

public class NominaDetalleDto
{
    public int Id { get; set; }
    public int EmpleadoId { get; set; }
    public string EmpleadoNombre { get; set; } = string.Empty;
    public string EmpleadoNumero { get; set; } = string.Empty;
    public int? ProyectoId { get; set; }
    public string? ProyectoNombre { get; set; }

    public decimal SueldoDiario { get; set; }
    public int DiasTrabajados { get; set; }
    public int Faltas { get; set; }
    public int Retardos { get; set; }
    public decimal HorasExtra { get; set; }

    public decimal SueldoBruto { get; set; }
    public decimal DescuentoInfonavit { get; set; }
    public decimal MontoAjuste { get; set; }
    public string? MotivoAjuste { get; set; }
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
    public decimal MontoAjuste { get; set; }
    public string? MotivoAjuste { get; set; }
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

// Un pago de nómina ya efectuado (NominaDetalle con Pagado = true), plano, para
// alimentar el dashboard de Gastos de Nómina (agrupación por fecha/empleado en el cliente).
public class PagoNominaDto
{
    public int EmpleadoId { get; set; }
    public string EmpleadoNombre { get; set; } = string.Empty;
    public int? ProyectoId { get; set; }
    public string? ProyectoNombre { get; set; }
    public decimal SueldoNeto { get; set; }
    public DateTime FechaPago { get; set; }
    public DateTime FechaInicioPeriodo { get; set; }
    public DateTime FechaFinPeriodo { get; set; }
}
