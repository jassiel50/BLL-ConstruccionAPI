using BLL_ConstruccionAPI.Models.Enums;
using BLL_ConstruccionAPI.Models.Inventario.Perdidas;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace BLL_ConstruccionAPI.Reports;

public class ReportePerdidasDocument : IDocument
{
    private readonly List<RegistroPerdida> _perdidas;
    private readonly DateTime              _desde;
    private readonly DateTime              _hasta;

    public ReportePerdidasDocument(List<RegistroPerdida> perdidas, DateTime desde, DateTime hasta)
    {
        _perdidas = perdidas;
        _desde    = desde;
        _hasta    = hasta;
    }

    public void Compose(IDocumentContainer container)
    {
        var total       = _perdidas.Count;
        var materiales  = _perdidas.Count(p => p.Tipo == TipoPerdida.Material);
        var herramientas = _perdidas.Count(p => p.Tipo == TipoPerdida.Herramienta);
        var subtitulo   = $"Período: {_desde:dd/MM/yyyy} — {_hasta:dd/MM/yyyy}";

        var agrupadas = _perdidas
            .GroupBy(p => p.Motivo)
            .OrderBy(g => g.Key.ToString());

        container.Page(page =>
        {
            page.Size(PageSizes.A4.Landscape());
            page.Margin(30);
            page.Header().Element(c => ReporteEstilos.AgregarEncabezado(c, "Reporte de Pérdidas", subtitulo));
            page.Footer().Element(ReporteEstilos.AgregarPiePagina);

            page.Content().PaddingTop(16).Column(col =>
            {
                // ─── Resumen ─────────────────────────────────────────────────
                col.Item().Row(row =>
                {
                    Tarjeta(row.RelativeItem(), "Total Pérdidas",      total.ToString(),        ReporteEstilos.ColorAlerta);
                    row.ConstantItem(12);
                    Tarjeta(row.RelativeItem(), "Pérdidas de Material",     materiales.ToString(),   ReporteEstilos.ColorAdvertencia);
                    row.ConstantItem(12);
                    Tarjeta(row.RelativeItem(), "Pérdidas de Herramienta",  herramientas.ToString(), ReporteEstilos.ColorSecundario);
                });

                col.Item().PaddingTop(20);

                // ─── Agrupado por motivo ──────────────────────────────────────
                foreach (var grupo in agrupadas)
                {
                    col.Item().Text($"Motivo: {grupo.Key}").FontSize(12).FontColor(ReporteEstilos.ColorPrimario).Bold();
                    col.Item().PaddingTop(6);

                    col.Item().Table(table =>
                    {
                        table.ColumnsDefinition(c =>
                        {
                            c.ConstantColumn(80);   // FECHA
                            c.ConstantColumn(80);   // TIPO
                            c.RelativeColumn(2);    // ELEMENTO
                            c.RelativeColumn(2);    // PROYECTO
                            c.ConstantColumn(80);   // CANTIDAD
                            c.RelativeColumn(3);    // DESCRIPCIÓN
                        });

                        table.Header(h =>
                        {
                            foreach (var t in new[] { "FECHA", "TIPO", "ELEMENTO", "PROYECTO", "CANTIDAD", "DESCRIPCIÓN" })
                                h.Cell().Background(ReporteEstilos.ColorPrimario).Padding(6)
                                    .Text(t).FontSize(9).FontColor(Colors.White).Bold();
                        });

                        bool alt = false;
                        foreach (var p in grupo.OrderByDescending(x => x.FechaPerdida))
                        {
                            string fondo = alt ? ReporteEstilos.ColorFondoTabla : "#ffffff";
                            alt = !alt;
                            var elemento = p.Tipo == TipoPerdida.Material
                                ? p.Material?.Nombre ?? "-"
                                : p.Herramienta?.Nombre ?? "-";

                            void Celda(string texto, bool negrita = false)
                            {
                                var txt = table.Cell().Background(fondo)
                                    .BorderBottom(0.5f).BorderColor(ReporteEstilos.ColorBordeTabla)
                                    .Padding(5).Text(texto).FontSize(8);
                                if (negrita) txt.Bold();
                            }

                            Celda(p.FechaPerdida.ToString("dd/MM/yyyy"));
                            Celda(p.Tipo.ToString());
                            Celda(elemento, true);
                            Celda(p.Proyecto?.Nombre ?? "-");
                            Celda(p.Tipo == TipoPerdida.Material ? p.CantidadPerdida.ToString("N2") : "—");
                            Celda(p.Descripcion);
                        }
                    });

                    col.Item().PaddingTop(14);
                }
            });
        });
    }

    private static void Tarjeta(IContainer container, string etiqueta, string valor, string color)
    {
        container.Background(color).Padding(12).Column(c =>
        {
            c.Item().Text(etiqueta).FontSize(9).FontColor(Colors.White);
            c.Item().Text(valor).FontSize(22).FontColor(Colors.White).Bold();
        });
    }
}
