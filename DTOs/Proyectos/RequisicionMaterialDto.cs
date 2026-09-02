namespace BLL_ConstruccionAPI.DTOs.Proyectos;

public class RequisicionMaterialDto
{
    public int Id { get; set; }
    public int ProyectoId { get; set; }
    public string ProyectoNombre { get; set; } = string.Empty;
    public int Folio { get; set; }
    public string SeRequierePara { get; set; } = string.Empty;
    public string SeSuministraPor { get; set; } = string.Empty;
    public DateTime FechaSolicitud { get; set; }
    public string SolicitoNombre { get; set; } = string.Empty;
    public DateTime FechaCreacion { get; set; }
    public int? FaseId { get; set; }
    public string? FaseNombre { get; set; }
    public List<RequisicionMaterialDetalleDto> Detalles { get; set; } = [];
}

public class RequisicionMaterialDetalleDto
{
    public int Id { get; set; }
    public int Orden { get; set; }
    public string Descripcion { get; set; } = string.Empty;
    public string Unidad { get; set; } = string.Empty;
    public decimal Cantidad { get; set; }
    public string? AreaComentarios { get; set; }
    public string Status { get; set; } = string.Empty;
    public int? MaterialId { get; set; }
    public string Responsable { get; set; } = string.Empty;
    public decimal CostoUnitario { get; set; }
    public int? GastoExtraId { get; set; }
}

public class RequisicionMaterialRequestDto
{
    public string SeRequierePara { get; set; } = string.Empty;
    public string SeSuministraPor { get; set; } = string.Empty;
    public DateTime FechaSolicitud { get; set; } = DateTime.UtcNow;
    public string SolicitoNombre { get; set; } = string.Empty;
    public int? FaseId { get; set; }
    public List<RequisicionMaterialDetalleRequestDto> Detalles { get; set; } = [];
}

public class RequisicionMaterialDetalleRequestDto
{
    public string Descripcion { get; set; } = string.Empty;
    public string Unidad { get; set; } = string.Empty;
    public decimal Cantidad { get; set; }
    public string? AreaComentarios { get; set; }
    public string Status { get; set; } = "Pendiente";
    public int? MaterialId { get; set; }
    public string Responsable { get; set; } = "Cliente";
    public decimal CostoUnitario { get; set; } = 0;
}
