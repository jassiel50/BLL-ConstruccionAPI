using BLL_ConstruccionAPI.Models.Enums;
using BLL_ConstruccionAPI.Models.Inventario.Proyectos;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace BLL_ConstruccionAPI.Reports;

public class ReporteProyectosDocument : IDocument
{
    private readonly List<Proyecto>     _proyectos;
    private readonly List<FaseProyecto> _fases;

    public ReporteProyectosDocument(List<Proyecto> proyectos, List<FaseProyecto> fases)
    {
        _proyectos = proyectos;
        _fases     = fases;
    }

    public void Compose(IDocumentContainer container)
    {
        var total     = _proyectos.Count;
        var activos   = _proyectos.Count(p => p.Estado == EstadoProyecto.Activo);
        var terminados = _proyectos.Count(p => p.Estado == EstadoProyecto.Terminado);
        var pausados  = _proyectos.Count(p => p.Estado == EstadoProyecto.Pausado);
        var hoy       = DateTime.UtcNow.Date;

        container.Page(page =>
        {
            page.Size(PageSizes.A4);
            page.Margin(30);
            page.Header().Element(c => ReporteEstilos.AgregarEncabezado(c, "Reporte de Proyectos"));
            page.Footer().Element(ReporteEstilos.AgregarPiePagina);

            page.Content().PaddingTop(16).Column(col =>
            {
                // ─── Resumen ─────────────────────────────────────────────────
                col.Item().Row(row =>
                {
                    Tarjeta(row.RelativeItem(), "Total",      total.ToString(),      ReporteEstilos.ColorPrimario);
                    row.ConstantItem(8);
                    Tarjeta(row.RelativeItem(), "Activos",    activos.ToString(),    ReporteEstilos.ColorExito);
                    row.ConstantItem(8);
                    Tarjeta(row.RelativeItem(), "Terminados", terminados.ToString(), ReporteEstilos.ColorGris);
                    row.ConstantItem(8);
                    Tarjeta(row.RelativeItem(), "Pausados",   pausados.ToString(),   ReporteEstilos.ColorAdvertencia);
                });

                col.Item().PaddingTop(20);

                // ─── Detalle por proyecto ─────────────────────────────────────
                foreach (var proyecto in _proyectos.OrderBy(p => p.Nombre))
                {
                    var fasesProyecto   = _fases.Where(f => f.ProyectoId == proyecto.Id).OrderBy(f => f.Orden).ToList();
                    var totalFases      = fasesProyecto.Count;
                    var fasesCompletadas = fasesProyecto.Count(f => f.Estado == EstadoFase.Completada);
                    var progreso        = totalFases > 0 ? (float)fasesCompletadas / totalFases : 0f;

                    string colorEstado = proyecto.Estado switch
                    {
                        EstadoProyecto.Activo    => ReporteEstilos.ColorExito,
                        EstadoProyecto.Terminado => ReporteEstilos.ColorGris,
                        _                        => ReporteEstilos.ColorAdvertencia
                    };

                    col.Item().Border(0.5f).BorderColor(ReporteEstilos.ColorBordeTabla)
                        .Padding(12).Column(pCol =>
                    {
                        // Encabezado del proyecto
                        pCol.Item().Row(row =>
                        {
                            row.RelativeItem().Column(c =>
                            {
                                c.Item().Text(proyecto.Nombre).FontSize(13).FontColor(ReporteEstilos.ColorPrimario).Bold();
                                c.Item().Text($"Cliente: {proyecto.Cliente?.Nombre ?? "-"}  |  Ubicación: {proyecto.Ubicacion}")
                                    .FontSize(9).FontColor(ReporteEstilos.ColorGris);
                                c.Item().Text($"Inicio: {proyecto.FechaInicio:dd/MM/yyyy}  |  Fin: {proyecto.FechaFin?.ToString("dd/MM/yyyy") ?? "—"}")
                                    .FontSize(9).FontColor(ReporteEstilos.ColorGris);
                            });
                            row.ConstantItem(80).AlignRight()
                                .Text(proyecto.Estado.ToString()).FontSize(10).FontColor(colorEstado).Bold();
                        });

                        // Barra de progreso
                        pCol.Item().PaddingTop(8).Column(c =>
                        {
                            c.Item().Text($"Progreso de fases: {fasesCompletadas}/{totalFases} ({progreso:P0})")
                                .FontSize(9).FontColor(ReporteEstilos.ColorGris);
                            c.Item().PaddingTop(3).Height(8).Row(row =>
                            {
                                if (progreso > 0)
                                    row.RelativeItem(progreso).Background(ReporteEstilos.ColorExito);
                                if (progreso < 1)
                                    row.RelativeItem(1 - progreso).Background(ReporteEstilos.ColorBordeTabla);
                            });
                        });

                        if (fasesProyecto.Count == 0)
                        {
                            pCol.Item().PaddingTop(8)
                                .Text("Sin fases registradas").FontSize(9).FontColor(ReporteEstilos.ColorAdvertencia).Italic();
                            return;
                        }

                        // Mini tabla de fases
                        pCol.Item().PaddingTop(10).Table(table =>
                        {
                            table.ColumnsDefinition(c =>
                            {
                                c.ConstantColumn(40);   // ORDEN
                                c.RelativeColumn(3);    // NOMBRE
                                c.ConstantColumn(100);  // FECHA LÍMITE
                                c.ConstantColumn(80);   // ESTADO
                            });

                            table.Header(h =>
                            {
                                foreach (var t in new[] { "ORDEN", "NOMBRE FASE", "FECHA LÍMITE", "ESTADO" })
                                    h.Cell().Background(ReporteEstilos.ColorSecundario).Padding(4)
                                        .Text(t).FontSize(8).FontColor(Colors.White).Bold();
                            });

                            foreach (var fase in fasesProyecto)
                            {
                                bool atrasada = fase.Estado != EstadoFase.Completada && fase.FechaLimite.Date < hoy;
                                string fondo  = atrasada ? "#fee2e2" : "#ffffff";

                                string colorFaseEstado = fase.Estado switch
                                {
                                    EstadoFase.Completada => ReporteEstilos.ColorExito,
                                    EstadoFase.Atrasada   => ReporteEstilos.ColorAlerta,
                                    EstadoFase.EnCurso    => ReporteEstilos.ColorSecundario,
                                    _                     => ReporteEstilos.ColorGris
                                };

                                void Celda(string texto, bool negrita = false)
                                {
                                    var txt = table.Cell().Background(fondo)
                                        .BorderBottom(0.5f).BorderColor(ReporteEstilos.ColorBordeTabla)
                                        .Padding(4).Text(texto).FontSize(8);
                                    if (negrita) txt.Bold();
                                }

                                Celda(fase.Orden.ToString());
                                Celda(fase.Nombre, true);
                                Celda(fase.FechaLimite.ToString("dd/MM/yyyy"));

                                table.Cell().Background(fondo)
                                    .BorderBottom(0.5f).BorderColor(ReporteEstilos.ColorBordeTabla)
                                    .Padding(4).Text(fase.Estado.ToString()).FontSize(8).FontColor(colorFaseEstado).Bold();
                            }
                        });
                    });

                    col.Item().PaddingTop(12);
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
