namespace BLL_ConstruccionAPI.DTOs.Reportes;

public class ReporteEntregaConformidadDto
{
    public int Id { get; set; }
    public int Folio { get; set; }
    public DateTime Fecha { get; set; }

    public int? ProyectoId { get; set; }
    public string? ProyectoNombreLibre { get; set; }
    public string ProyectoNombre { get; set; } = string.Empty;

    public int? ClienteId { get; set; }
    public string? EmpresaNombreLibre { get; set; }
    public string EmpresaNombre { get; set; } = string.Empty;
    public string? ContactoNombre { get; set; }

    public string Concepto { get; set; } = string.Empty;
    public string? OrdenCompra { get; set; }
    public string Descripcion { get; set; } = string.Empty;
    public string? CondicionesEntrega { get; set; }

    public string EntregaNombre { get; set; } = string.Empty;
    public string EntregaEmpresa { get; set; } = string.Empty;
    public string? RecibeNombre { get; set; }
    public string? RecibeEmpresa { get; set; }

    public bool TieneDocumentoFirmado { get; set; }
    public DateTime? DocumentoFirmadoFecha { get; set; }

    public DateTime FechaCreacion { get; set; }
    public List<ReporteEntregaConformidadFotoDto> Fotos { get; set; } = [];
}

public class ReporteEntregaConformidadFotoDto
{
    public int Id { get; set; }
    public string NombreOriginal { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
}

public class ReporteEntregaConformidadRequestDto
{
    public DateTime Fecha { get; set; } = DateTime.UtcNow;

    public int? ProyectoId { get; set; }
    public string? ProyectoNombreLibre { get; set; }
    public int? ClienteId { get; set; }
    public string? EmpresaNombreLibre { get; set; }
    public string? ContactoNombre { get; set; }

    public string Concepto { get; set; } = string.Empty;
    public string? OrdenCompra { get; set; }
    public string Descripcion { get; set; } = string.Empty;
    public string? CondicionesEntrega { get; set; }

    public string? EntregaNombre { get; set; }
    public string? EntregaEmpresa { get; set; }
    public string? RecibeNombre { get; set; }
    public string? RecibeEmpresa { get; set; }
}
