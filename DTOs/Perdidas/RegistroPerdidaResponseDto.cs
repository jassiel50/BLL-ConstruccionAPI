using BLL_ConstruccionAPI.Models.Inventario.Perdidas;

namespace BLL_ConstruccionAPI.DTOs.Perdidas;

public class RegistroPerdidaResponseDto
{
    public int Id { get; set; }
    public string Tipo { get; set; } = string.Empty;
    public int? MaterialId { get; set; }
    public string NombreMaterial { get; set; } = string.Empty;
    public int? HerramientaId { get; set; }
    public string NombreHerramienta { get; set; } = string.Empty;
    public int? ProyectoId { get; set; }
    public string NombreProyecto { get; set; } = string.Empty;
    public string Motivo { get; set; } = string.Empty;
    public decimal CantidadPerdida { get; set; }
    public string Descripcion { get; set; } = string.Empty;
    public DateTime FechaPerdida { get; set; }
    public DateTime FechaRegistro { get; set; }

    public static RegistroPerdidaResponseDto FromEntity(RegistroPerdida r) => new()
    {
        Id                = r.Id,
        Tipo              = r.Tipo.ToString(),
        MaterialId        = r.MaterialId,
        NombreMaterial    = r.Material?.Nombre ?? string.Empty,
        HerramientaId     = r.HerramientaId,
        NombreHerramienta = r.Herramienta?.Nombre ?? string.Empty,
        ProyectoId        = r.ProyectoId,
        NombreProyecto    = r.Proyecto?.Nombre ?? string.Empty,
        Motivo            = r.Motivo.ToString(),
        CantidadPerdida   = r.CantidadPerdida,
        Descripcion       = r.Descripcion,
        FechaPerdida      = r.FechaPerdida,
        FechaRegistro     = r.FechaRegistro
    };
}
