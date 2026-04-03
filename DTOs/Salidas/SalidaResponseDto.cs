using BLL_ConstruccionAPI.Models.Inventario.Materiales;

namespace BLL_ConstruccionAPI.DTOs.Salidas;

public class SalidaResponseDto
{
    public int Id { get; set; }
    public string NumeroFolio { get; set; } = string.Empty;
    public int ProyectoId { get; set; }
    public string NombreProyecto { get; set; } = string.Empty;
    public int UsuarioId { get; set; }
    public DateTime Fecha { get; set; }
    public string Observaciones { get; set; } = string.Empty;
    public List<SalidaDetalleResponseDto> Detalles { get; set; } = [];

    public static SalidaResponseDto FromEntity(Salida e) => new()
    {
        Id = e.Id,
        NumeroFolio = e.NumeroFolio,
        ProyectoId = e.ProyectoId,
        NombreProyecto = e.Proyecto?.Nombre ?? string.Empty,
        UsuarioId = e.UsuarioId,
        Fecha = e.Fecha,
        Observaciones = e.Observaciones,
        Detalles = e.Detalles.Select(SalidaDetalleResponseDto.FromEntity).ToList()
    };
}
