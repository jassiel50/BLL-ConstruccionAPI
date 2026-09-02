using BLL_ConstruccionAPI.Models.Inventario;

namespace BLL_ConstruccionAPI.Models.Cotizaciones;

public class Cotizacion
{
    public int Id { get; set; }

    public string Folio { get; set; } = string.Empty;

    // "Borrador" mientras se captura (se autoguarda), "Generada" una vez finalizada con folio y PDF.
    public string Estado { get; set; } = "Generada";

    public int? ClienteId { get; set; }
    // Respaldo si el cliente no está en el catálogo (mismo patrón que Servicio.ClienteNombre).
    public string? EmpresaNombreLibre { get; set; }
    public string? ContactoNombre { get; set; }

    public string Titulo { get; set; } = string.Empty;
    public string? Introduccion { get; set; }
    public string? AlcanceGeneral { get; set; }

    public DateTime FechaCotizacion { get; set; } = DateTime.UtcNow;
    public int? TiempoEntregaDias { get; set; }
    public string? Clausulas { get; set; }
    public int ValidezDias { get; set; } = 8;
    public string CondicionesPago { get; set; } = string.Empty;
    public string MetodoPago { get; set; } = string.Empty;

    public decimal Subtotal { get; set; }
    public decimal Iva { get; set; }
    public decimal Total { get; set; }

    public byte[] PdfContenido { get; set; } = [];

    public int CreadoPorId { get; set; }
    public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;

    public Cliente? Cliente { get; set; }
    public ICollection<CotizacionItem> Items { get; set; } = new List<CotizacionItem>();
}
