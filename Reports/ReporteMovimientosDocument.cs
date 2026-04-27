using BLL_ConstruccionAPI.Models.Inventario.Materiales;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace BLL_ConstruccionAPI.Reports;

public class ReporteMovimientosDocument : IDocument
{
    private readonly List<Entrada>           _entradas;
    private readonly List<Salida>            _salidas;
    private readonly List<DevolucionMaterial> _devoluciones;
    private readonly DateTime                _desde;
    private readonly DateTime                _hasta;

    public ReporteMovimientosDocument(
        List<Entrada> entradas,
        List<Salida> salidas,
        List<DevolucionMaterial> devoluciones,
        DateTime desde,
        DateTime hasta)
    {
        _entradas     = entradas;
        _salidas      = salidas;
        _devoluciones = devoluciones;
        _desde        = desde;
        _hasta        = hasta;
    }

    public void Compose(IDocumentContainer container)
    {
        decimal totalEntradas     = _entradas.Sum(e => e.Detalles.Sum(d => d.Cantidad));
        decimal totalSalidas      = _salidas.Sum(s => s.Detalles.Sum(d => d.Cantidad));
        decimal totalDevoluciones = _devoluciones.Sum(d => d.CantidadDevuelta);
        decimal balanceNeto       = totalEntradas + totalDevoluciones - totalSalidas;

        var subtitulo = $"Período: {_desde:dd/MM/yyyy} — {_hasta:dd/MM/yyyy}";

        container.Page(page =>
        {
            page.Size(PageSizes.A4.Landscape());
            page.Margin(30);
            page.Header().Element(c => ReporteEstilos.AgregarEncabezado(c, "Reporte de Movimientos de Stock", subtitulo));
            page.Footer().Element(ReporteEstilos.AgregarPiePagina);

            page.Content().PaddingTop(16).Column(col =>
            {
                // ─── Resumen ─────────────────────────────────────────────────
                col.Item().Row(row =>
                {
                    Tarjeta(row.RelativeItem(), "Entradas",     totalEntradas.ToString("N2"),     ReporteEstilos.ColorExito);
                    row.ConstantItem(10);
                    Tarjeta(row.RelativeItem(), "Salidas",      totalSalidas.ToString("N2"),      ReporteEstilos.ColorAlerta);
                    row.ConstantItem(10);
                    Tarjeta(row.RelativeItem(), "Devoluciones", totalDevoluciones.ToString("N2"), ReporteEstilos.ColorSecundario);
                    row.ConstantItem(10);
                    Tarjeta(row.RelativeItem(), "Balance Neto", balanceNeto.ToString("N2"),
                        balanceNeto >= 0 ? ReporteEstilos.ColorExito : ReporteEstilos.ColorAlerta);
                });

                col.Item().PaddingTop(20);

                // ─── Entradas ────────────────────────────────────────────────
                col.Item().Text("Entradas de Inventario").FontSize(13).FontColor(ReporteEstilos.ColorPrimario).Bold();
                col.Item().PaddingTop(6);
                col.Item().Table(table =>
                {
                    table.ColumnsDefinition(c =>
                    {
                        c.ConstantColumn(80);
                        c.RelativeColumn(3);
                        c.ConstantColumn(80);
                        c.RelativeColumn(2);
                    });
                    EncabezadoTabla(table, ["FECHA", "MATERIAL", "CANTIDAD", "PROVEEDOR"]);
                    bool alt = false;
                    foreach (var e in _entradas.OrderByDescending(x => x.Fecha))
                        foreach (var d in e.Detalles)
                        {
                            string f = alt ? ReporteEstilos.ColorFondoTabla : "#ffffff";
                            alt = !alt;
                            FilaCelda(table, f, e.Fecha.ToString("dd/MM/yyyy"));
                            FilaCelda(table, f, d.Material?.Nombre ?? "-", true);
                            FilaCelda(table, f, d.Cantidad.ToString("N2"));
                            FilaCelda(table, f, e.Proveedor?.Nombre ?? "-");
                        }
                });

                col.Item().PaddingTop(16);

                // ─── Salidas ─────────────────────────────────────────────────
                col.Item().Text("Salidas de Inventario").FontSize(13).FontColor(ReporteEstilos.ColorPrimario).Bold();
                col.Item().PaddingTop(6);
                col.Item().Table(table =>
                {
                    table.ColumnsDefinition(c =>
                    {
                        c.ConstantColumn(80);
                        c.RelativeColumn(3);
                        c.ConstantColumn(80);
                        c.RelativeColumn(2);
                    });
                    EncabezadoTabla(table, ["FECHA", "MATERIAL", "CANTIDAD", "PROYECTO"]);
                    bool alt = false;
                    foreach (var s in _salidas.OrderByDescending(x => x.Fecha))
                        foreach (var d in s.Detalles)
                        {
                            string f = alt ? ReporteEstilos.ColorFondoTabla : "#ffffff";
                            alt = !alt;
                            FilaCelda(table, f, s.Fecha.ToString("dd/MM/yyyy"));
                            FilaCelda(table, f, d.Material?.Nombre ?? "-", true);
                            FilaCelda(table, f, d.Cantidad.ToString("N2"));
                            FilaCelda(table, f, s.Proyecto?.Nombre ?? "-");
                        }
                });

                col.Item().PaddingTop(16);

                // ─── Devoluciones ────────────────────────────────────────────
                col.Item().Text("Devoluciones de Material").FontSize(13).FontColor(ReporteEstilos.ColorPrimario).Bold();
                col.Item().PaddingTop(6);
                col.Item().Table(table =>
                {
                    table.ColumnsDefinition(c =>
                    {
                        c.ConstantColumn(80);
                        c.RelativeColumn(2);
                        c.ConstantColumn(80);
                        c.RelativeColumn(2);
                        c.RelativeColumn(3);
                    });
                    EncabezadoTabla(table, ["FECHA", "MATERIAL", "CANTIDAD", "PROYECTO", "OBSERVACIONES"]);
                    bool alt = false;
                    foreach (var d in _devoluciones.OrderByDescending(x => x.FechaDevolucion))
                    {
                        string f = alt ? ReporteEstilos.ColorFondoTabla : "#ffffff";
                        alt = !alt;
                        FilaCelda(table, f, d.FechaDevolucion.ToString("dd/MM/yyyy"));
                        FilaCelda(table, f, d.Material?.Nombre ?? "-", true);
                        FilaCelda(table, f, d.CantidadDevuelta.ToString("N2"));
                        FilaCelda(table, f, d.Proyecto?.Nombre ?? "-");
                        FilaCelda(table, f, d.Observaciones);
                    }
                });
            });
        });
    }

    private static void Tarjeta(IContainer container, string etiqueta, string valor, string color)
    {
        container.Background(color).Padding(12).Column(c =>
        {
            c.Item().Text(etiqueta).FontSize(9).FontColor(Colors.White);
            c.Item().Text(valor).FontSize(20).FontColor(Colors.White).Bold();
        });
    }

    private static void EncabezadoTabla(TableDescriptor table, string[] cols)
    {
        table.Header(h =>
        {
            foreach (var c in cols)
                h.Cell().Background(ReporteEstilos.ColorPrimario).Padding(6)
                    .Text(c).FontSize(9).FontColor(Colors.White).Bold();
        });
    }

    private static void FilaCelda(TableDescriptor table, string fondo, string texto, bool negrita = false)
    {
        var t = table.Cell().Background(fondo)
            .BorderBottom(0.5f).BorderColor(ReporteEstilos.ColorBordeTabla)
            .Padding(5).Text(texto).FontSize(8);
        if (negrita) t.Bold();
    }
}
