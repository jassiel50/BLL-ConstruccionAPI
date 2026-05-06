using BLL_ConstruccionAPI.Models.Inventario.Proyectos;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace BLL_ConstruccionAPI.Reports;

public class PlaneacionDocument : IDocument
{
    private readonly Proyecto _proyecto;
    private readonly List<FaseProyecto> _fases;

    public PlaneacionDocument(Proyecto proyecto, List<FaseProyecto> fases)
    {
        _proyecto = proyecto;
        _fases = fases;
    }

    public void Compose(IDocumentContainer container)
    {
        container.Page(page =>
        {
            page.Size(PageSizes.A4);
            page.Margin(35);
            page.Header().Element(c => ReporteEstilos.AgregarEncabezado(c, "Plan de Ejecución de Proyecto"));
            page.Footer().Element(ReporteEstilos.AgregarPiePagina);

            page.Content().PaddingTop(16).Column(col =>
            {
                // ─── Datos del proyecto ───────────────────────────────────────
                col.Item().Border(0.5f).BorderColor(ReporteEstilos.ColorBordeTabla).Table(table =>
                {
                    table.ColumnsDefinition(c =>
                    {
                        c.ConstantColumn(130);
                        c.RelativeColumn();
                        c.ConstantColumn(130);
                        c.RelativeColumn();
                    });

                    void Etiqueta(string texto) =>
                        table.Cell().Background(ReporteEstilos.ColorSecundario).Padding(6)
                            .Text(texto).FontSize(8).FontColor(Colors.White).Bold();

                    void Valor(string texto) =>
                        table.Cell().Background(Colors.White).Padding(6)
                            .Text(texto).FontSize(9);

                    Etiqueta("Proyecto:"); Valor(_proyecto.Nombre);
                    Etiqueta("Cliente:"); Valor(_proyecto.Cliente?.Nombre ?? "-");
                    Etiqueta("Ubicación:"); Valor(_proyecto.Ubicacion);
                    Etiqueta("Estado:"); Valor(_proyecto.Estado.ToString());
                    Etiqueta("Fecha Inicio:"); Valor(_proyecto.FechaInicio.ToString("dd/MM/yyyy"));
                    Etiqueta("Fecha Fin Est.:"); Valor(_proyecto.FechaFin?.ToString("dd/MM/yyyy") ?? "Por definir");
                    Etiqueta("N° Cotización:"); Valor(string.IsNullOrEmpty(_proyecto.NumeroCotizacion) ? "-" : _proyecto.NumeroCotizacion);
                    Etiqueta("Orden de Compra:"); Valor(string.IsNullOrEmpty(_proyecto.OrdenCompra) ? "-" : _proyecto.OrdenCompra);
                });

                col.Item().PaddingTop(14);

                // ─── Texto de compromiso ──────────────────────────────────────
                col.Item().Background(ReporteEstilos.ColorFondoTabla)
                    .Border(0.5f).BorderColor(ReporteEstilos.ColorBordeTabla)
                    .Padding(12)
                    .Text("El presente documento avala el compromiso de ejecución del proyecto conforme a las fases y fechas establecidas a continuación. Los involucrados se comprometen a cumplir con los tiempos acordados.")
                    .FontSize(9).Italic().FontColor(ReporteEstilos.ColorGris);

                col.Item().PaddingTop(14);

                // ─── Tabla de fases ───────────────────────────────────────────
                col.Item().Text("Fases del Proyecto").FontSize(11).FontColor(ReporteEstilos.ColorPrimario).Bold();
                col.Item().PaddingTop(6).Table(table =>
                {
                    table.ColumnsDefinition(c =>
                    {
                        c.ConstantColumn(30);   // #
                        c.RelativeColumn(2);    // Fase
                        c.RelativeColumn(3);    // Descripción
                        c.ConstantColumn(90);   // Fecha Límite
                    });

                    table.Header(h =>
                    {
                        foreach (var t in new[] { "#", "Fase", "Descripción", "Fecha Límite" })
                            h.Cell().Background(ReporteEstilos.ColorSecundario).Padding(5)
                                .Text(t).FontSize(8).FontColor(Colors.White).Bold();
                    });

                    bool alterno = false;
                    foreach (var fase in _fases.OrderBy(f => f.Orden))
                    {
                        string fondo = alterno ? ReporteEstilos.ColorFondoTabla : Colors.White;
                        alterno = !alterno;

                        void Celda(string texto, bool bold = false)
                        {
                            var txt = table.Cell().Background(fondo)
                                .BorderBottom(0.5f).BorderColor(ReporteEstilos.ColorBordeTabla)
                                .Padding(5).Text(texto).FontSize(8);
                            if (bold) txt.Bold();
                        }

                        Celda(fase.Orden.ToString());
                        Celda(fase.Nombre, true);
                        Celda(fase.Descripcion);
                        Celda(fase.FechaLimite.ToString("dd/MM/yyyy"));
                    }
                });

                col.Item().PaddingTop(30);

                // ─── Sección de firmas ────────────────────────────────────────
                col.Item().Text("Firmas de Conformidad").FontSize(11).FontColor(ReporteEstilos.ColorPrimario).Bold();
                col.Item().PaddingTop(6).Text("Al firmar este documento, los participantes aceptan y se comprometen a cumplir con el plan de ejecución establecido.")
                    .FontSize(8).FontColor(ReporteEstilos.ColorGris).Italic();

                col.Item().PaddingTop(30).Row(row =>
                {
                    // Firma 1
                    row.RelativeItem().Column(c =>
                    {
                        c.Item().PaddingBottom(30).Text("").FontSize(8);
                        c.Item().LineHorizontal(1).LineColor(ReporteEstilos.ColorPrimario);
                        c.Item().PaddingTop(4).Text("Responsable del Proyecto").FontSize(8).FontColor(ReporteEstilos.ColorGris);
                        c.Item().Text("Nombre: ___________________________").FontSize(8).FontColor(ReporteEstilos.ColorGris);
                    });

                    row.ConstantItem(30);

                    // Firma 2
                    row.RelativeItem().Column(c =>
                    {
                        c.Item().PaddingBottom(30).Text("").FontSize(8);
                        c.Item().LineHorizontal(1).LineColor(ReporteEstilos.ColorPrimario);
                        c.Item().PaddingTop(4).Text("Supervisor / Administración").FontSize(8).FontColor(ReporteEstilos.ColorGris);
                        c.Item().Text("Nombre: ___________________________").FontSize(8).FontColor(ReporteEstilos.ColorGris);
                    });
                });

                col.Item().PaddingTop(30).Row(row =>
                {
                    row.RelativeItem().Column(c =>
                    {
                        c.Item().PaddingBottom(30).Text("").FontSize(8);
                        c.Item().LineHorizontal(1).LineColor(ReporteEstilos.ColorPrimario);
                        c.Item().PaddingTop(4).Text("Fecha de Aceptación").FontSize(8).FontColor(ReporteEstilos.ColorGris);
                    });
                    row.RelativeItem();
                });
            });
        });
    }
}
