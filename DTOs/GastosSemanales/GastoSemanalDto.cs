namespace BLL_ConstruccionAPI.DTOs.GastosSemanales;

public class GastoSemanalDto
{
    public int Id { get; set; }
    public int ProyectoId { get; set; }
    public string Concepto { get; set; } = string.Empty;
    public decimal Monto { get; set; }
    public DateTime FechaInicio { get; set; }
    public DateTime FechaFin { get; set; }
    public string Tipo { get; set; } = string.Empty;
    public string? Observaciones { get; set; }
    public int NumPersonas { get; set; }
    public DateTime FechaRegistro { get; set; }
    public int DiasDesdeUltimo { get; set; }
    public int? PeriodoNominaId { get; set; }
}

public class GastoSemanalRequestDto
{
    public string Concepto { get; set; } = string.Empty;
    public decimal Monto { get; set; }
    public DateTime FechaInicio { get; set; }
    public DateTime FechaFin { get; set; }
    public string Tipo { get; set; } = "Semanal";
    public string? Observaciones { get; set; }
    public int NumPersonas { get; set; } = 0;
}

// Periodo de nómina ya generado que tiene empleados asignados a este proyecto,
// disponible para heredar su monto real como gasto semanal (en vez de capturarlo a mano).
public class NominaDisponibleProyectoDto
{
    public int PeriodoNominaId { get; set; }
    public DateTime FechaInicio { get; set; }
    public DateTime FechaFin { get; set; }
    public decimal MontoTotal { get; set; }
    public int NumEmpleados { get; set; }
    public List<EmpleadoMontoDto> Empleados { get; set; } = [];
}

public class EmpleadoMontoDto
{
    public string EmpleadoNombre { get; set; } = string.Empty;
    public decimal SueldoNeto { get; set; }
}

public class CrearGastoDesdeNominaDto
{
    public int PeriodoNominaId { get; set; }
}
