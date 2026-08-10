using BLL_ConstruccionAPI.Models.Inventario.Proyectos;
using BLL_ConstruccionAPI.Models.Personal;

namespace BLL_ConstruccionAPI.Models.Nomina;

public class NominaDetalle
{
    public int Id { get; set; }

    public int PeriodoNominaId { get; set; }
    public int EmpleadoId { get; set; }
    public int? ProyectoId { get; set; }

    // Desglose de asistencia usado para calcular SueldoBruto (snapshot histórico:
    // no cambia si luego se edita el sueldo semanal del empleado).
    public decimal SueldoDiario { get; set; }
    public int DiasTrabajados { get; set; }
    public int Faltas { get; set; }
    public int Retardos { get; set; }
    public decimal HorasExtra { get; set; }

    public decimal SueldoBruto { get; set; }
    public decimal DescuentoInfonavit { get; set; }
    // Ajuste manual capturado al generar el periodo: positivo (bono, hora extra) o
    // negativo (descuento por falta, retardo, etc.).
    public decimal MontoAjuste { get; set; }
    public string? MotivoAjuste { get; set; }
    public decimal SueldoNeto { get; set; }

    public bool Pagado { get; set; }
    public DateTime? FechaPago { get; set; }

    public PeriodoNomina? PeriodoNomina { get; set; }
    public Empleado? Empleado { get; set; }
    public Proyecto? Proyecto { get; set; }
}
