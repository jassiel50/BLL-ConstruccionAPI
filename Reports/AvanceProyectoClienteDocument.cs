using BLL_ConstruccionAPI.DTOs.Fases;
using BLL_ConstruccionAPI.Models.Inventario.Proyectos;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace BLL_ConstruccionAPI.Reports;

/// <summary>
/// Reporte de avance de proyecto pensado para compartirse con el cliente: solo
/// progreso general de fases y checklist de actividades. No incluye fechas límite
/// por fase, estado de atraso, ni ningún dato financiero (contrato, pagos, saldo,
/// presupuesto, gasto real ni utilidad).
/// </summary>
public class AvanceProyectoClienteDocument : IDocument
{
    private readonly Proyecto _proyecto;
    private readonly List<FaseResponseDto> _fases;

    public AvanceProyectoClienteDocument(Proyecto proyecto, List<FaseResponseDto> fases)
    {
        _proyecto = proyecto;
        _fases = fases;
    }

    public void Compose(IDocumentContainer container)
    {
        var totalFases = _fases.Count;
        var completadas = _fases.Count(f => f.Estado == "Completada");
        var pendientes = totalFases - completadas;
        var porcentaje = totalFases > 0 ? (double)completadas / totalFases * 100 : 0;

        container.Page(page =>
        {
            page.Size(PageSizes.A4);
            page.Margin(30);
            page.Header().Element(c => ReporteEstilos.AgregarEncabezado(c,
                "Reporte de Avance de Proyecto",
                $"Proyecto: {_proyecto.Nombre}"));
            page.Footer().Element(ReporteEstilos.AgregarPiePagina);

            page.Content().PaddingTop(16).Column(col =>
            {
                // ─── Datos generales ──────────────────────────────────────
                col.Item().Border(0.5f).BorderColor(ReporteEstilos.ColorBordeTabla).Table(table =>
                {
                    table.ColumnsDefinition(c =>
                    {
                        c.ConstantColumn(110);
                        c.RelativeColumn();
                        c.ConstantColumn(110);
                        c.RelativeColumn();
                    });

                    void Etiqueta(string texto) =>
                        table.Cell().Background(ReporteEstilos.ColorSecundario).Padding(6)
                            .Text(texto).FontSize(8).FontColor(Colors.White).Bold();
                    void Valor(string texto) =>
                        table.Cell().Background(Colors.White).Padding(6).Text(texto).FontSize(9);

                    Etiqueta("Cliente:"); Valor(_proyecto.Cliente?.Nombre ?? "-");
                    Etiqueta("Ubicación:"); Valor(_proyecto.Ubicacion);
                    Etiqueta("Fecha Inicio:"); Valor(_proyecto.FechaInicio.ToString("dd/MM/yyyy"));
                    Etiqueta("Estado:"); Valor(_proyecto.Estado.ToString());
                });

                col.Item().PaddingTop(16);

                // ─── Avance general ───────────────────────────────────────
                col.Item().Row(row =>
                {
                    InfoBox(row.RelativeItem(), "Avance General", $"{porcentaje:N0}%", ReporteEstilos.ColorPrimario);
                    row.ConstantItem(8);
                    InfoBox(row.RelativeItem(), "Fases Completadas", $"{completadas} de {totalFases}", ReporteEstilos.ColorExito);
                    row.ConstantItem(8);
                    InfoBox(row.RelativeItem(), "Fases Pendientes", pendientes.ToString(), ReporteEstilos.ColorGris);
                });

                col.Item().PaddingTop(18);

                // ─── Tabla de fases ───────────────────────────────────────
                col.Item().Text("Seguimiento de Fases").FontSize(11).Bold().FontColor(ReporteEstilos.ColorPrimario);
                col.Item().PaddingTop(6).Table(table =>
                {
                    table.ColumnsDefinition(c =>
                    {
                        c.ConstantColumn(22);
                        c.RelativeColumn(3);
                        c.RelativeColumn(5);
                        c.ConstantColumn(90);
                    });

                    table.Header(h =>
                    {
                        foreach (var t in new[] { "#", "Fase", "Descripción", "Estado" })
                            h.Cell().Background(ReporteEstilos.ColorPrimario).Padding(6)
                                .Text(t).FontSize(8).Bold().FontColor("#FFFFFF");
                    });

                    var num = 1;
                    foreach (var fase in _fases.OrderBy(f => f.Orden))
                    {
                        var bg = num % 2 == 0 ? ReporteEstilos.ColorFondoTabla : "#FFFFFF";
                        var (estadoTexto, estadoColor) = EstadoVisual(fase);

                        table.Cell().Background(bg).Padding(5).Text(num.ToString()).FontSize(8).FontColor(ReporteEstilos.ColorGris);
                        table.Cell().Background(bg).Padding(5).Text(fase.Nombre).FontSize(8).Bold();
                        table.Cell().Background(bg).Padding(5).Text(fase.Descripcion).FontSize(8);
                        table.Cell().Background(bg).Padding(5).Text(estadoTexto).FontSize(8).Bold().FontColor(estadoColor);
                        num++;
                    }

                    if (_fases.Count == 0)
                    {
                        table.Cell().ColumnSpan(4).Padding(12)
                            .Text("Sin fases registradas.").FontSize(9).FontColor(ReporteEstilos.ColorGris).Italic();
                    }
                });

                // ─── Checklist de actividades por fase ────────────────────
                var fasesConChecklist = _fases.Where(f => f.Checklist.Count > 0).OrderBy(f => f.Orden).ToList();
                if (fasesConChecklist.Count > 0)
                {
                    col.Item().PaddingTop(20);
                    col.Item().Text("Checklist de Actividades").FontSize(11).Bold().FontColor(ReporteEstilos.ColorPrimario);

                    foreach (var fase in fasesConChecklist)
                    {
                        var completados = fase.Checklist.Count(c => c.Completado);
                        col.Item().PaddingTop(8).Row(row =>
                        {
                            row.RelativeItem().Text(fase.Nombre).FontSize(9).Bold().FontColor(ReporteEstilos.ColorSecundario);
                            row.AutoItem().Text($"{completados} de {fase.Checklist.Count}").FontSize(8).FontColor(ReporteEstilos.ColorGris);
                        });

                        col.Item().PaddingTop(3).Column(items =>
                        {
                            foreach (var item in fase.Checklist)
                            {
                                items.Item().Row(itemRow =>
                                {
                                    itemRow.AutoItem().Width(14).Text(item.Completado ? "✔" : "○")
                                        .FontSize(9).FontColor(item.Completado ? ReporteEstilos.ColorExito : ReporteEstilos.ColorGris);
                                    itemRow.RelativeItem().Text(item.Descripcion).FontSize(8.5f)
                                        .FontColor(item.Completado ? ReporteEstilos.ColorGris : ReporteEstilos.ColorPrimario);
                                });
                            }
                        });
                    }
                }
            });
        });
    }

    // Simplificado a propósito: el cliente solo ve si una fase ya se completó o
    // sigue en curso, sin exponer atrasos, fechas límite ni retrasos.
    private static (string Texto, string Color) EstadoVisual(FaseResponseDto f) =>
        f.Estado == "Completada"
            ? ("Completada", ReporteEstilos.ColorExito)
            : ("En curso", ReporteEstilos.ColorGris);

    private static void InfoBox(IContainer container, string label, string valor, string color)
    {
        container.Border(1).BorderColor(color).Padding(8).Column(c =>
        {
            c.Item().Text(label).FontSize(8).FontColor(ReporteEstilos.ColorGris);
            c.Item().Text(valor).FontSize(13).Bold().FontColor(color);
        });
    }
}
