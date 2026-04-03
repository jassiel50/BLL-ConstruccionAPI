using BLL_ConstruccionAPI.Models.Inventario.Cátalogos;

namespace BLL_ConstruccionAPI.DTOs.Catalogos;

public class UnidadMedidaResponseDto
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Abreviatura { get; set; } = string.Empty;
    public bool Activo { get; set; }

    public static UnidadMedidaResponseDto FromEntity(UnidadMedida e) => new()
    {
        Id = e.Id,
        Nombre = e.Nombre,
        Abreviatura = e.Abreviatura,
        Activo = e.Activo
    };
}
