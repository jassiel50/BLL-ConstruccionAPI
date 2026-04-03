using System.ComponentModel.DataAnnotations;

namespace BLL_ConstruccionAPI.DTOs.Herramientas;

public class AsignacionRequestDto
{
    [Range(1, int.MaxValue)]
    public int HerramientaId { get; set; }

    [Range(1, int.MaxValue)]
    public int ProyectoId { get; set; }

    public int? UsuarioRecibeId { get; set; }

    [StringLength(500)]
    public string Observaciones { get; set; } = string.Empty;
}
