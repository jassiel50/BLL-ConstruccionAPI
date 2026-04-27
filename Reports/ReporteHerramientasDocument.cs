using BLL_ConstruccionAPI.Models.Enums;
using BLL_ConstruccionAPI.Models.Inventario.Herramientas;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace BLL_ConstruccionAPI.Reports;

public class ReporteHerramientasDocument : IDocument
{
    private readonly List<Herramienta>           _herramientas;
    private readonly List<AsignacionHerramienta> _asignaciones;

    public ReporteHerramientasDocument(
        List<Herramienta> herramientas,
        List<AsignacionHerramienta> asignaciones)
    {
        _herramientas = herramientas;
        _asignaciones = asignaciones;
    }

    public void Compose(IDocumentContainer container)
    {
        var total         = _herramientas.Count;
        var disponibles   = _herramientas.Count(h => h.Estado == EstadoHerramienta.Disponible);
        var asignadas     = _herramientas.Count(h => h.Estado == EstadoHerramienta.Asignada);
        var mantenimiento = _herramientas.Count(h => h.Estado == EstadoHerramienta.Mantenimiento);

        container.Page(page =>
        {
            page.Size(PageSizes.A4.Landscape());
            page.Margin(30);
            page.Header().Element(c => ReporteEstilos.AgregarEncabezado(c, "Reporte de Herramientas"));
            page.Footer().Element(ReporteEstilos.AgregarPiePagina);

            page.Content().PaddingTop(16).Column(col =>
            {
                // ─── Resumen ─────────────────────────────────────────────────
                col.Item().Row(row =>
                {
                    Tarjeta(row.RelativeItem(), "Total",         total.ToString(),         ReporteEstilos.ColorPrimario);
                    row.ConstantItem(10);
                    Tarjeta(row.RelativeItem(), "Disponibles",   disponibles.ToString(),   ReporteEstilos.ColorExito);
                    row.ConstantItem(10);
                    Tarjeta(row.RelativeItem(), "Asignadas",     asignadas.ToString(),     ReporteEstilos.ColorSecundario);
                    row.ConstantItem(10);
                    Tarjeta(row.RelativeItem(), "Mantenimiento", mantenimiento.ToString(), ReporteEstilos.ColorAdvertencia);
                });

                col.Item().PaddingTop(20);

                // ─── Tabla herramientas ───────────────────────────────────────
                col.Item().Text("Inventario de Herramientas").FontSize(13).FontColor(ReporteEstilos.ColorPrimario).Bold();
                col.Item().PaddingTop(6);
                col.Item().Table(table =>
                {
                    table.ColumnsDefinition(c =>
                    {
                        c.ConstantColumn(70);   // CÓDIGO
                        c.RelativeColumn(3);    // NOMBRE
                        c.RelativeColumn(2);    // CATEGORÍA
                        c.ConstantColumn(80);   // ZONA
                        c.ConstantColumn(90);   // ESTADO
                        c.ConstantColumn(90);   // VALOR
                    });

                    table.Header(h =>
                    {
                        foreach (var t in new[] { "CÓDIGO", "NOMBRE", "CATEGORÍA", "ZONA", "ESTADO", "VALOR" })
                            h.Cell().Background(ReporteEstilos.ColorPrimario).Padding(6)
                                .Text(t).FontSize(9).FontColor(Colors.White).Bold();
                    });

                    bool alt = false;
                    foreach (var h in _herramientas.OrderBy(x => x.Nombre))
                    {
                        string fondo = alt ? ReporteEstilos.ColorFondoTabla : "#ffffff";
                        alt = !alt;

                        string colorEstado = h.Estado switch
                        {
                            EstadoHerramienta.Disponible   => ReporteEstilos.ColorExito,
                            EstadoHerramienta.Asignada     => ReporteEstilos.ColorSecundario,
                            EstadoHerramienta.Mantenimiento => ReporteEstilos.ColorAdvertencia,
                            _                               => ReporteEstilos.ColorAlerta
                        };

                        void Celda(string texto, bool negrita = false)
                        {
                            var txt = table.Cell().Background(fondo)
                                .BorderBottom(0.5f).BorderColor(ReporteEstilos.ColorBordeTabla)
                                .Padding(5).Text(texto).FontSize(8);
                            if (negrita) txt.Bold();
                        }

                        Celda(h.Codigo);
                        Celda(h.Nombre, true);
                        Celda(h.CategoriaHerramienta?.Nombre ?? "-");
                        Celda(h.Zona.ToString());

                        table.Cell().Background(fondo)
                            .BorderBottom(0.5f).BorderColor(ReporteEstilos.ColorBordeTabla)
                            .Padding(5).Text(h.Estado.ToString()).FontSize(8).FontColor(colorEstado).Bold();

                        Celda($"${h.ValorAdquisicion:N2}");
                    }
                });

                col.Item().PaddingTop(20);

                // ─── Asignaciones activas ────────────────────────────────────
                col.Item().Text("Asignaciones Activas").FontSize(13).FontColor(ReporteEstilos.ColorPrimario).Bold();
                col.Item().PaddingTop(6);
                col.Item().Table(table =>
                {
                    table.ColumnsDefinition(c =>
                    {
                        c.RelativeColumn(3);   // HERRAMIENTA
                        c.RelativeColumn(3);   // PROYECTO
                        c.ConstantColumn(110); // FECHA ASIGNACIÓN
                        c.ConstantColumn(90);  // DÍAS ASIGNADA
                    });

                    table.Header(h =>
                    {
                        foreach (var t in new[] { "HERRAMIENTA", "PROYECTO", "FECHA ASIGNACIÓN", "DÍAS ASIGNADA" })
                            h.Cell().Background(ReporteEstilos.ColorPrimario).Padding(6)
                                .Text(t).FontSize(9).FontColor(Colors.White).Bold();
                    });

                    bool alt = false;
                    foreach (var a in _asignaciones.OrderBy(x => x.FechaAsignacion))
                    {
                        string fondo = alt ? ReporteEstilos.ColorFondoTabla : "#ffffff";
                        alt = !alt;
                        var dias = (DateTime.UtcNow - a.FechaAsignacion).Days;

                        void Celda(string texto, bool negrita = false)
                        {
                            var txt = table.Cell().Background(fondo)
                                .BorderBottom(0.5f).BorderColor(ReporteEstilos.ColorBordeTabla)
                                .Padding(5).Text(texto).FontSize(8);
                            if (negrita) txt.Bold();
                        }

                        Celda(a.Herramienta?.Nombre ?? "-", true);
                        Celda(a.Proyecto?.Nombre    ?? "-");
                        Celda(a.FechaAsignacion.ToString("dd/MM/yyyy"));

                        var colorDias = dias > 15 ? ReporteEstilos.ColorAdvertencia : ReporteEstilos.ColorGris;
                        var tDias = table.Cell().Background(fondo)
                            .BorderBottom(0.5f).BorderColor(ReporteEstilos.ColorBordeTabla)
                            .Padding(5).Text($"{dias} días").FontSize(8).FontColor(colorDias);
                        if (dias > 15) tDias.Bold();
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
            c.Item().Text(valor).FontSize(22).FontColor(Colors.White).Bold();
        });
    }
}
