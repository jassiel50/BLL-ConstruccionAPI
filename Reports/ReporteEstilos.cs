using QuestPDF.Fluent;
using QuestPDF.Infrastructure;

namespace BLL_ConstruccionAPI.Reports;

public static class ReporteEstilos
{
    // Colores
    public static readonly string ColorPrimario    = "#002046";
    public static readonly string ColorSecundario  = "#1b365d";
    public static readonly string ColorAlerta      = "#ba1a1a";
    public static readonly string ColorExito       = "#166534";
    public static readonly string ColorAdvertencia = "#92400e";
    public static readonly string ColorGris        = "#64748b";
    public static readonly string ColorFondoTabla  = "#f8fafc";
    public static readonly string ColorBordeTabla  = "#e2e8f0";

    public static void AgregarEncabezado(IContainer container, string titulo, string subtitulo = "")
    {
        container.Column(col =>
        {
            col.Item().Row(row =>
            {
                row.RelativeItem().Column(c =>
                {
                    c.Item().Text("B.L.L. Servicios y Proyectos Industriales")
                        .FontSize(9).FontColor(ColorGris).Bold();
                    c.Item().Text(titulo)
                        .FontSize(20).FontColor(ColorPrimario).Bold();
                    if (!string.IsNullOrEmpty(subtitulo))
                        c.Item().Text(subtitulo).FontSize(10).FontColor(ColorGris);
                });
                row.ConstantItem(120).AlignRight().Column(c =>
                {
                    c.Item().Text($"Generado: {DateTime.Now:dd/MM/yyyy HH:mm}")
                        .FontSize(8).FontColor(ColorGris);
                });
            });
            col.Item().PaddingTop(8).LineHorizontal(2).LineColor(ColorPrimario);
        });
    }

    public static void AgregarPiePagina(IContainer container)
    {
        container.Row(row =>
        {
            row.RelativeItem().Text("B.L.L. Servicios y Proyectos Industriales — Documento confidencial")
                .FontSize(8).FontColor(ColorGris);
            row.ConstantItem(80).AlignRight()
                .Text(x =>
                {
                    x.Span("Página ").FontSize(8).FontColor(ColorGris);
                    x.CurrentPageNumber().FontSize(8).FontColor(ColorGris);
                    x.Span(" de ").FontSize(8).FontColor(ColorGris);
                    x.TotalPages().FontSize(8).FontColor(ColorGris);
                });
        });
    }
}
