namespace BLL_ConstruccionAPI.DTOs.Cotizaciones;

public class CotizacionRequestDto
{
    public int? ClienteId { get; set; }
    public string? EmpresaNombreLibre { get; set; }
    public string? ContactoNombre { get; set; }

    public string Titulo { get; set; } = string.Empty;
    public string? Introduccion { get; set; }
    public string? AlcanceGeneral { get; set; }

    public int? TiempoEntregaDias { get; set; }
    public string? Clausulas { get; set; }
    public int ValidezDias { get; set; } = 8;
    public string CondicionesPago { get; set; } = string.Empty;
    public string MetodoPago { get; set; } = string.Empty;

    public List<CotizacionItemDto> Items { get; set; } = [];
}

public class CotizacionItemDto
{
    public string Descripcion { get; set; } = string.Empty;
    public string Cantidad { get; set; } = string.Empty;
    public string Unidad { get; set; } = string.Empty;
    public decimal Total { get; set; }
}

public class CotizacionResponseDto
{
    public int Id { get; set; }
    public string Folio { get; set; } = string.Empty;
    public string Empresa { get; set; } = string.Empty;
    public string? ContactoNombre { get; set; }
    public string Titulo { get; set; } = string.Empty;
    public DateTime FechaCotizacion { get; set; }
    public decimal Total { get; set; }
}
