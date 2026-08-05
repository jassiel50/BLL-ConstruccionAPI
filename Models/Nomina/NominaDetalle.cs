using BLL_ConstruccionAPI.Models.Inventario.Proyectos;
using BLL_ConstruccionAPI.Models.Personal;

namespace BLL_ConstruccionAPI.Models.Nomina;

public class NominaDetalle
{
    public int Id { get; set; }

    public int PeriodoNominaId { get; set; }
    public int EmpleadoId { get; set; }
    public int? ProyectoId { get; set; }

    public decimal SueldoBruto { get; set; }
    public decimal DescuentoInfonavit { get; set; }
    public decimal SueldoNeto { get; set; }

    public bool Pagado { get; set; }
    public DateTime? FechaPago { get; set; }

    public PeriodoNomina? PeriodoNomina { get; set; }
    public Empleado? Empleado { get; set; }
    public Proyecto? Proyecto { get; set; }
}
