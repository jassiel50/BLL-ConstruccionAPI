using BLL_ConstruccionAPI.Models.Reportes;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace BLL_ConstruccionAPI.Reports;

/// <summary>
/// Reporte de entrega y conformidad: datos de la entrega, condiciones y evidencias,
/// con tabla ENTREGA/RECIBE con líneas de firma en blanco — se imprime, se firma en
/// papel por ambas partes y el documento firmado se sube escaneado por separado.
/// </summary>
public class ReporteEntregaConformidadDocument : IDocument
{
    private readonly ReporteEntregaConformidad _reporte;

    public ReporteEntregaConformidadDocument(ReporteEntregaConformidad reporte)
    {
        _reporte = reporte;
    }

    private static string ValorOGuion(string? valor) => string.IsNullOrWhiteSpace(valor) ? "—" : valor;

    private string NombreProyecto =>
        _reporte.ProyectoId.HasValue && _reporte.Proyecto is not null ? _reporte.Proyecto.Nombre : ValorOGuion(_reporte.ProyectoNombreLibre);

    private string NombreEmpresa =>
        _reporte.ClienteId.HasValue && _reporte.Cliente is not null ? _reporte.Cliente.Nombre : ValorOGuion(_reporte.EmpresaNombreLibre);

    public void Compose(IDocumentContainer container)
    {
        container.Page(page =>
        {
            page.Size(PageSizes.A4);
            page.Margin(35);
            page.DefaultTextStyle(t => t.FontSize(9.5f).FontFamily("Arial"));

            page.Header().Element(c => ReporteEstilos.AgregarEncabezado(c, "REPORTE DE ENTREGA Y CONFORMIDAD",
                $"Folio #E-{_reporte.Folio:D4}  ·  Fecha: {_reporte.Fecha:dd/MM/yyyy}", conLogo: true));

            page.Footer().Element(ReporteEstilos.AgregarPiePagina);

            page.Content().PaddingTop(14).Column(col =>
            {
                col.Spacing(10);

                // ── 1. Datos de la entrega ────────────────────────────────
                Seccion(col, "1. Datos de la Entrega");
                DatoFila(col, "Proyecto", NombreProyecto);
                DatoFila(col, "Empresa / Cliente", NombreEmpresa);
                if (!string.IsNullOrWhiteSpace(_reporte.ContactoNombre))
                    DatoFila(col, "Contacto", _reporte.ContactoNombre!);
                DatoFila(col, "Concepto", ValorOGuion(_reporte.Concepto));
                if (!string.IsNullOrWhiteSpace(_reporte.OrdenCompra))
                    DatoFila(col, "Orden de compra", _reporte.OrdenCompra!);

                // ── 2. Descripción ────────────────────────────────────────
                Seccion(col, "2. Descripción");
                col.Item().Text(ValorOGuion(_reporte.Descripcion));

                // ── 3. Condiciones de entrega ─────────────────────────────
                if (!string.IsNullOrWhiteSpace(_reporte.CondicionesEntrega))
                {
                    Seccion(col, "3. Condiciones de Entrega");
                    col.Item().Column(items =>
                    {
                        foreach (var linea in DividirLineas(_reporte.CondicionesEntrega))
                            items.Item().Row(r =>
                            {
                                r.ConstantItem(12).Text("•").FontColor(ReporteEstilos.ColorPrimario);
                                r.RelativeItem().Text(linea).FontSize(9);
                            });
                    });
                }

                // ── 4. Notas adicionales ───────────────────────────────────
                if (!string.IsNullOrWhiteSpace(_reporte.TextoAdicional))
                {
                    Seccion(col, "4. Notas Adicionales");
                    col.Item().Text(_reporte.TextoAdicional);
                }

                // ── 5. Evidencias ──────────────────────────────────────────
                Seccion(col, "5. Evidencias");
                if (_reporte.Fotos.Count == 0)
                {
                    col.Item().Text("Sin evidencias.").FontColor(ReporteEstilos.ColorGris);
                }
                else
                {
                    foreach (var fila in _reporte.Fotos.Chunk(3))
                    {
                        col.Item().Row(row =>
                        {
                            foreach (var foto in fila)
                                row.RelativeItem().Padding(2).Height(140).Image(foto.Contenido).FitArea();
                        });
                    }
                }

                // ── 6. Conformidad de entrega ──────────────────────────────
                Seccion(col, "6. Conformidad de Entrega");
                col.Item().PaddingTop(4).Row(row =>
                {
                    row.RelativeItem().Column(c => ColumnaFirma(c, "ENTREGA", _reporte.EntregaNombre, _reporte.EntregaEmpresa, _reporte.Fecha.ToString("dd/MM/yyyy")));
                    row.ConstantItem(20);
                    row.RelativeItem().Column(c => ColumnaFirma(c, "RECIBE", ValorOGuion(_reporte.RecibeNombre), ValorOGuion(_reporte.RecibeEmpresa), null));
                });
            });
        });
    }

    private static void ColumnaFirma(QuestPDF.Fluent.ColumnDescriptor col, string titulo, string nombre, string empresa, string? fecha)
    {
        col.Item().Text(titulo).Bold().FontSize(10).FontColor(ReporteEstilos.ColorPrimario);
        col.Item().PaddingTop(4).Text(t =>
        {
            t.Span("Nombre: ").Bold();
            t.Span(nombre);
        });
        col.Item().Text(t =>
        {
            t.Span("Empresa: ").Bold();
            t.Span(empresa);
        });
        col.Item().PaddingTop(28).LineHorizontal(0.75f).LineColor(ReporteEstilos.ColorBordeTabla);
        col.Item().Text("Firma").FontSize(7.5f).FontColor(ReporteEstilos.ColorGris);
        col.Item().PaddingTop(10).Text(t =>
        {
            t.Span("Fecha: ").Bold();
            t.Span(fecha ?? "________________");
        });
    }

    private static void Seccion(QuestPDF.Fluent.ColumnDescriptor col, string titulo)
    {
        col.Item().PaddingTop(6).Text(titulo).Bold().FontSize(11).FontColor(ReporteEstilos.ColorPrimario);
        col.Item().PaddingBottom(2).LineHorizontal(0.75f).LineColor(ReporteEstilos.ColorBordeTabla);
    }

    private static void DatoFila(QuestPDF.Fluent.ColumnDescriptor col, string etiqueta, string valor)
    {
        col.Item().Row(row =>
        {
            row.ConstantItem(160).Text(etiqueta).FontColor(ReporteEstilos.ColorGris);
            row.RelativeItem().Text(valor);
        });
    }

    private static IEnumerable<string> DividirLineas(string? texto) =>
        (texto ?? string.Empty)
            .Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Where(l => l.Length > 0);
}
