using BLL_ConstruccionAPI.DTOs.Pagos;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace BLL_ConstruccionAPI.Reports;

public class PagosDocument : IDocument
{
    private readonly ResumenPagosDto _resumen;

    public PagosDocument(ResumenPagosDto resumen)
    {
        _resumen = resumen;
    }

    public void Compose(IDocumentContainer container)
    {
        container.Page(page =>
        {
            page.Size(PageSizes.A4);
            page.Margin(30);
            page.Header().Element(c => ReporteEstilos.AgregarEncabezado(c,
                "Control de Pagos",
                $"Proyecto: {_resumen.NombreProyecto}"));
            page.Footer().Element(ReporteEstilos.AgregarPiePagina);

            page.Content().PaddingTop(16).Column(col =>
            {
                // ─── Resumen financiero ───────────────────────────────────
                col.Item().Border(1).BorderColor(ReporteEstilos.ColorBordeTabla)
                    .Padding(12).Column(resCol =>
                    {
                        resCol.Item().Text("Resumen Financiero")
                            .FontSize(11).Bold().FontColor(ReporteEstilos.ColorPrimario);
                        resCol.Item().PaddingTop(8).Row(row =>
                        {
                            InfoBox(row.RelativeItem(), "Monto Contrato",
                                $"${_resumen.MontoContrato:N2}", ReporteEstilos.ColorPrimario);
                            row.ConstantItem(8);
                            InfoBox(row.RelativeItem(), "Total Pagado",
                                $"${_resumen.TotalPagado:N2}", ReporteEstilos.ColorExito);
                            row.ConstantItem(8);
                            var colorSaldo = _resumen.SaldoPendiente <= 0
                                ? ReporteEstilos.ColorExito
                                : ReporteEstilos.ColorAdvertencia;
                            InfoBox(row.RelativeItem(), "Saldo Pendiente",
                                $"${_resumen.SaldoPendiente:N2}", colorSaldo);
                            row.ConstantItem(8);
                            InfoBox(row.RelativeItem(), "Núm. Pagos",
                                _resumen.NumeroPagos.ToString(), ReporteEstilos.ColorGris);
                        });
                    });

                col.Item().PaddingTop(16);

                // ─── Tabla de pagos ───────────────────────────────────────
                col.Item().Text("Detalle de Pagos")
                    .FontSize(11).Bold().FontColor(ReporteEstilos.ColorPrimario);
                col.Item().PaddingTop(6).Table(table =>
                {
                    table.ColumnsDefinition(cols =>
                    {
                        cols.ConstantColumn(30);
                        cols.RelativeColumn(3);
                        cols.RelativeColumn(2);
                        cols.RelativeColumn(2);
                        cols.RelativeColumn(2);
                        cols.RelativeColumn(3);
                    });

                    // Encabezado
                    table.Header(header =>
                    {
                        var headerCells = new[] { "#", "Concepto", "Monto", "Fecha Pago", "Método", "Referencia" };
                        foreach (var h in headerCells)
                            header.Cell().Background(ReporteEstilos.ColorPrimario)
                                .Padding(6).Text(h).FontSize(8).Bold().FontColor("#FFFFFF");
                    });

                    // Filas
                    var numero = 1;
                    foreach (var pago in _resumen.Pagos)
                    {
                        var bg = numero % 2 == 0 ? ReporteEstilos.ColorFondoTabla : "#FFFFFF";
                        table.Cell().Background(bg).Padding(5)
                            .Text(numero.ToString()).FontSize(8).FontColor(ReporteEstilos.ColorGris);
                        table.Cell().Background(bg).Padding(5)
                            .Text(pago.Concepto).FontSize(8);
                        table.Cell().Background(bg).Padding(5)
                            .Text($"${pago.Monto:N2}").FontSize(8);
                        table.Cell().Background(bg).Padding(5)
                            .Text(pago.FechaPago.ToString("dd/MM/yyyy")).FontSize(8);
                        table.Cell().Background(bg).Padding(5)
                            .Text(pago.MetodoPago).FontSize(8);
                        table.Cell().Background(bg).Padding(5)
                            .Text(pago.Referencia).FontSize(8).FontColor(ReporteEstilos.ColorGris);
                        numero++;
                    }

                    if (!_resumen.Pagos.Any())
                    {
                        table.Cell().ColumnSpan(6).Padding(12)
                            .Text("Sin pagos registrados.").FontSize(9)
                            .FontColor(ReporteEstilos.ColorGris).Italic();
                    }
                });

                // ─── Pie con porcentaje cubierto ──────────────────────────
                if (_resumen.MontoContrato > 0)
                {
                    var porcentaje = (_resumen.TotalPagado / _resumen.MontoContrato) * 100;
                    col.Item().PaddingTop(12).Text(
                        $"Porcentaje cubierto: {porcentaje:N1}% del contrato")
                        .FontSize(9).Italic().FontColor(ReporteEstilos.ColorGris);
                }
            });
        });
    }

    private static void InfoBox(IContainer container, string label, string valor, string color)
    {
        container.Border(1).BorderColor(color).Padding(8).Column(c =>
        {
            c.Item().Text(label).FontSize(8).FontColor(ReporteEstilos.ColorGris);
            c.Item().Text(valor).FontSize(13).Bold().FontColor(color);
        });
    }
}
