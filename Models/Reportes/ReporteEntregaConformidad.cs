using BLL_ConstruccionAPI.Models.Inventario;
using BLL_ConstruccionAPI.Models.Inventario.Proyectos;

namespace BLL_ConstruccionAPI.Models.Reportes;

/// <summary>
/// Reporte de entrega de una pieza/material ya fabricado al cliente, con firma de quien
/// entrega (BLL) y quien recibe (cliente). Se genera el PDF, se imprime y firma en papel,
/// y luego se sube el documento ya firmado escaneado para quedar con el registro.
/// </summary>
public class ReporteEntregaConformidad
{
    public int Id { get; set; }
    public int Folio { get; set; }
    public DateTime Fecha { get; set; } = DateTime.UtcNow;

    // Proyecto/Cliente del catálogo (opcional) o capturados libremente si no están dados de alta.
    public int? ProyectoId { get; set; }
    public string? ProyectoNombreLibre { get; set; }
    public int? ClienteId { get; set; }
    public string? EmpresaNombreLibre { get; set; }
    public string? ContactoNombre { get; set; }

    public string Concepto { get; set; } = string.Empty;
    public string? OrdenCompra { get; set; }
    public string Descripcion { get; set; } = string.Empty;
    // Una condición por línea; se muestran como viñetas en el PDF.
    public string? CondicionesEntrega { get; set; }
    // Texto libre opcional para notas u observaciones adicionales del reporte.
    public string? TextoAdicional { get; set; }

    public string EntregaNombre { get; set; } = "Ing. Baldemar López";
    public string EntregaEmpresa { get; set; } = "Servicios y Proyectos Industriales BLL";
    public string? RecibeNombre { get; set; }
    public string? RecibeEmpresa { get; set; }

    // Documento ya firmado en papel, subido escaneado después de la entrega.
    public string? DocumentoFirmadoNombre { get; set; }
    public string? DocumentoFirmadoContentType { get; set; }
    public byte[]? DocumentoFirmadoContenido { get; set; }
    public DateTime? DocumentoFirmadoFecha { get; set; }

    public int CreadoPorId { get; set; }
    public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;

    public Proyecto? Proyecto { get; set; }
    public Cliente? Cliente { get; set; }
    public List<ReporteEntregaConformidadFoto> Fotos { get; set; } = [];
}
