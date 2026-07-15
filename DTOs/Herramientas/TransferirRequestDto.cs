using System.ComponentModel.DataAnnotations;

namespace BLL_ConstruccionAPI.DTOs.Herramientas;

public class TransferirRequestDto
{
    [Range(1, int.MaxValue)]
    public int AsignacionId { get; set; }

    [Range(1, int.MaxValue)]
    public int NuevoProyectoId { get; set; }
}

public class TransferirMultipleRequestDto
{
    public List<int> AsignacionIds { get; set; } = new();

    [Range(1, int.MaxValue)]
    public int NuevoProyectoId { get; set; }
}

public class DevolverMultipleRequestDto
{
    public List<int> AsignacionIds { get; set; } = new();

    [StringLength(500)]
    public string ObservacionesDevolucion { get; set; } = string.Empty;
}

public class CambiarUbicacionRequestDto
{
    [Required]
    public string TipoUbicacion { get; set; } = string.Empty;
}

public class AsignacionOperacionResultDto
{
    public int AsignacionId { get; set; }
    public int ResponseCode { get; set; }
    public string ResponseMsg { get; set; } = string.Empty;
}
