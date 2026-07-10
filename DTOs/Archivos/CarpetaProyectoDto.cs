namespace BLL_ConstruccionAPI.DTOs.Archivos;

public class CarpetaProyectoDto
{
    public int Id { get; set; }
    public int ProyectoId { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public DateTime FechaCreacion { get; set; }
    public int NumeroArchivos { get; set; }
}

public class CarpetaProyectoRequestDto
{
    public string Nombre { get; set; } = string.Empty;
}
