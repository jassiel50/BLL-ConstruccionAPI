using BLL_ConstruccionAPI.DTOs.GastosExtras;
using BLL_ConstruccionAPI.Models.Inventario.Proyectos;

namespace BLL_ConstruccionAPI.DTOs.Fases;

public class FaseResponseDto
{
    public int Id { get; set; }
    public int ProyectoId { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Descripcion { get; set; } = string.Empty;
    public int Orden { get; set; }
    public DateTime FechaLimite { get; set; }
    public DateTime? FechaCompletada { get; set; }
    public string Estado { get; set; } = string.Empty;
    public bool Atrasada => Estado != "Completada" && FechaLimite.Date < DateTime.UtcNow.Date.AddDays(-1);

    public bool PorVencer => Estado != "Completada"
        && FechaLimite.Date >= DateTime.UtcNow.Date
        && FechaLimite.Date <= DateTime.UtcNow.Date.AddDays(2);

    public decimal GastoExtra { get; set; } = 0;
    public List<GastoExtraDto> GastosExtras { get; set; } = [];

    public static FaseResponseDto FromEntity(FaseProyecto f) => new()
    {
        Id = f.Id,
        ProyectoId = f.ProyectoId,
        Nombre = f.Nombre,
        Descripcion = f.Descripcion,
        Orden = f.Orden,
        FechaLimite = f.FechaLimite,
        FechaCompletada = f.FechaCompletada,
        Estado = f.Estado.ToString()
    };
}
