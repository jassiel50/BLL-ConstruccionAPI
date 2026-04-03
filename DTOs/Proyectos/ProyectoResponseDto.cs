using BLL_ConstruccionAPI.Models.Inventario.Proyectos;

namespace BLL_ConstruccionAPI.DTOs.Proyectos;

public class ProyectoResponseDto
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Descripcion { get; set; } = string.Empty;
    public string Ubicacion { get; set; } = string.Empty;
    public int ClienteId { get; set; }
    public string NombreCliente { get; set; } = string.Empty;
    public int ResponsableId { get; set; }
    public DateTime FechaInicio { get; set; }
    public DateTime? FechaFin { get; set; }
    public string Estado { get; set; } = string.Empty;
    public DateTime FechaRegistro { get; set; }

    public static ProyectoResponseDto FromEntity(Proyecto e) => new()
    {
        Id = e.Id,
        Nombre = e.Nombre,
        Descripcion = e.Descripcion,
        Ubicacion = e.Ubicacion,
        ClienteId = e.ClienteId,
        NombreCliente = e.Cliente?.Nombre ?? string.Empty,
        ResponsableId = e.ResponsableId,
        FechaInicio = e.FechaInicio,
        FechaFin = e.FechaFin,
        Estado = e.Estado.ToString(),
        FechaRegistro = e.FechaRegistro
    };
}
