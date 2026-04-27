using BLL_ConstruccionAPI.Models.Inventario.Materiales;
using QuestPDF.Elements.Table;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace BLL_ConstruccionAPI.Reports;

public class ReporteInventarioDocument : IDocument
{
    private readonly List<AlmacenCentral> _stock;

    public ReporteInventarioDocument(List<AlmacenCentral> stock)
    {
        _stock = stock;
    }

    public void Compose(IDocumentContainer container)
    {
        var totalMateriales  = _stock.Count;
        var stockCritico     = _stock.Count(s => s.Stock == 0);
        var stockBajo        = _stock.Count(s => s.Stock > 0 && s.Material != null && s.Stock <= s.Material.StockMinimo);

        container.Page(page =>
        {
            page.Size(PageSizes.A4.Landscape());
            page.Margin(30);
            page.Header().Element(c => ReporteEstilos.AgregarEncabezado(c, "Reporte de Inventario General"));
            page.Footer().Element(ReporteEstilos.AgregarPiePagina);

            page.Content().PaddingTop(16).Column(col =>
            {
                // ─── Tarjetas resumen ────────────────────────────────────────
                col.Item().Row(row =>
                {
                    Tarjeta(row.RelativeItem(), "Total Materiales", totalMateriales.ToString(), ReporteEstilos.ColorPrimario);
                    row.ConstantItem(12);
                    Tarjeta(row.RelativeItem(), "Stock Crítico (sin existencia)", stockCritico.ToString(), ReporteEstilos.ColorAlerta);
                    row.ConstantItem(12);
                    Tarjeta(row.RelativeItem(), "Stock Bajo", stockBajo.ToString(), ReporteEstilos.ColorAdvertencia);
                });

                col.Item().PaddingTop(20);

                // ─── Tabla ───────────────────────────────────────────────────
                col.Item().Table(table =>
                {
                    table.ColumnsDefinition(c =>
                    {
                        c.ConstantColumn(70);   // CÓDIGO
                        c.RelativeColumn(3);    // MATERIAL
                        c.RelativeColumn(2);    // CATEGORÍA
                        c.ConstantColumn(90);   // STOCK ACTUAL
                        c.ConstantColumn(90);   // STOCK MÍNIMO
                        c.ConstantColumn(80);   // ESTADO
                    });

                    // Encabezado de tabla
                    static void CeldaHeader(ITableCellContainer c, string texto) =>
                        c.Background(ReporteEstilos.ColorPrimario)
                         .Padding(6)
                         .Text(texto).FontSize(9).FontColor(Colors.White).Bold();

                    table.Header(h =>
                    {
                        CeldaHeader(h.Cell(), "CÓDIGO");
                        CeldaHeader(h.Cell(), "MATERIAL");
                        CeldaHeader(h.Cell(), "CATEGORÍA");
                        CeldaHeader(h.Cell(), "STOCK ACTUAL");
                        CeldaHeader(h.Cell(), "STOCK MÍNIMO");
                        CeldaHeader(h.Cell(), "ESTADO");
                    });

                    bool alterno = false;
                    foreach (var item in _stock.OrderBy(s => s.Material?.Nombre))
                    {
                        string fondo = alterno ? ReporteEstilos.ColorFondoTabla : "#ffffff";
                        alterno = !alterno;

                        string estado, colorEstado;
                        if (item.Stock == 0)
                        { estado = "Crítico"; colorEstado = ReporteEstilos.ColorAlerta; }
                        else if (item.Material != null && item.Stock <= item.Material.StockMinimo)
                        { estado = "Bajo"; colorEstado = ReporteEstilos.ColorAdvertencia; }
                        else
                        { estado = "OK"; colorEstado = ReporteEstilos.ColorExito; }

                        void Celda(ITableCellContainer c, string texto, bool negrita = false)
                        {
                            var t = c.Background(fondo).BorderBottom(0.5f).BorderColor(ReporteEstilos.ColorBordeTabla)
                                .Padding(5).Text(texto).FontSize(8);
                            if (negrita) t.Bold();
                        }

                        Celda(table.Cell(), item.Material?.Codigo ?? "-");
                        Celda(table.Cell(), item.Material?.Nombre ?? "-", true);
                        Celda(table.Cell(), item.Material?.Categoria?.Nombre ?? "-");
                        Celda(table.Cell(), item.Stock.ToString("N2"));
                        Celda(table.Cell(), item.Material?.StockMinimo.ToString("N2") ?? "-");

                        table.Cell().Background(fondo)
                            .BorderBottom(0.5f).BorderColor(ReporteEstilos.ColorBordeTabla)
                            .Padding(5)
                            .Text(estado).FontSize(8).FontColor(colorEstado).Bold();
                    }
                });
            });
        });
    }

    private static void Tarjeta(IContainer container, string etiqueta, string valor, string color)
    {
        container.Background(color).Padding(14).Column(c =>
        {
            c.Item().Text(etiqueta).FontSize(9).FontColor(Colors.White);
            c.Item().Text(valor).FontSize(24).FontColor(Colors.White).Bold();
        });
    }
}
