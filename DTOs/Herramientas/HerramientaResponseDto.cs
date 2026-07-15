using BLL_ConstruccionAPI.Models.Inventario.Herramientas;

namespace BLL_ConstruccionAPI.DTOs.Herramientas;

public class HerramientaResponseDto
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Descripcion { get; set; } = string.Empty;
    public string Codigo { get; set; } = string.Empty;
    public string NumeroSerie { get; set; } = string.Empty;
    public int CategoriaHerramientaId { get; set; }
    public string NombreCategoria { get; set; } = string.Empty;
    public string Estado { get; set; } = string.Empty;
    public decimal ValorAdquisicion { get; set; }
    public DateTime FechaAdquisicion { get; set; }
    public int Cantidad { get; set; }
    public int CantidadDisponible { get; set; }
    public string Zona { get; set; } = string.Empty;
    public string TipoUbicacion { get; set; } = string.Empty;

    public static HerramientaResponseDto FromEntity(Herramienta e, int? cantidadDisponible = null) => new()
    {
        Id = e.Id,
        Nombre = e.Nombre,
        Descripcion = e.Descripcion,
        Codigo = e.Codigo,
        NumeroSerie = e.NumeroSerie,
        CategoriaHerramientaId = e.CategoriaHerramientaId,
        NombreCategoria = e.CategoriaHerramienta?.Nombre ?? string.Empty,
        Estado = e.Estado.ToString(),
        ValorAdquisicion = e.ValorAdquisicion,
        FechaAdquisicion = e.FechaAdquisicion,
        Cantidad = e.Cantidad,
        CantidadDisponible = cantidadDisponible ?? e.Cantidad,
        Zona = e.Zona.ToString(),
        TipoUbicacion = e.TipoUbicacion
    };
}
