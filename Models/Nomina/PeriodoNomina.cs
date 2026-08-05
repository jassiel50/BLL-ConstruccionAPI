using BLL_ConstruccionAPI.Models.Enums;

namespace BLL_ConstruccionAPI.Models.Nomina;

public class PeriodoNomina
{
    public int Id { get; set; }

    public DateTime FechaInicio { get; set; }
    public DateTime FechaFin { get; set; }
    public DateTime FechaGeneracion { get; set; } = DateTime.UtcNow;
    public int GeneradoPorId { get; set; }
    public EstadoPeriodoNomina Estado { get; set; } = EstadoPeriodoNomina.Generada;

    public ICollection<NominaDetalle> Detalles { get; set; } = new List<NominaDetalle>();
}
