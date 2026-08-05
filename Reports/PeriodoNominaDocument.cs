using BLL_ConstruccionAPI.DTOs.Nomina;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace BLL_ConstruccionAPI.Reports;

public class PeriodoNominaDocument : IDocument
{
    private readonly PeriodoNominaDto _periodo;

    public PeriodoNominaDocument(PeriodoNominaDto periodo)
    {
        _periodo = periodo;
    }

    public void Compose(IDocumentContainer container)
    {
        container.Page(page =>
        {
            page.Size(PageSizes.A4);
            page.Margin(30);
            page.Header().Element(c => ReporteEstilos.AgregarEncabezado(c,
                "Reporte de Nómina",
                $"Periodo {_periodo.FechaInicio:dd/MM/yyyy} — {_periodo.FechaFin:dd/MM/yyyy}"));
            page.Footer().Element(ReporteEstilos.AgregarPiePagina);

            page.Content().PaddingTop(16).Column(col =>
            {
                col.Item().Border(1).BorderColor(ReporteEstilos.ColorBordeTabla)
                    .Padding(12).Row(row =>
                    {
                        InfoBox(row.RelativeItem(), "Empleados", _periodo.TotalEmpleados.ToString(), ReporteEstilos.ColorPrimario);
                        row.ConstantItem(8);
                        InfoBox(row.RelativeItem(), "Total Bruto", $"${_periodo.TotalBruto:N2}", ReporteEstilos.ColorGris);
                        row.ConstantItem(8);
                        InfoBox(row.RelativeItem(), "Descuentos", $"${_periodo.TotalDescuentos:N2}", ReporteEstilos.ColorAdvertencia);
                        row.ConstantItem(8);
                        InfoBox(row.RelativeItem(), "Total Neto", $"${_periodo.TotalNeto:N2}", ReporteEstilos.ColorExito);
                        row.ConstantItem(8);
                        InfoBox(row.RelativeItem(), "Estado", _periodo.Estado,
                            _periodo.Estado == "Pagada" ? ReporteEstilos.ColorExito : ReporteEstilos.ColorAdvertencia);
                    });

                col.Item().PaddingTop(16);

                col.Item().Text("Detalle por empleado")
                    .FontSize(11).Bold().FontColor(ReporteEstilos.ColorPrimario);
                col.Item().PaddingTop(6).Table(table =>
                {
                    table.ColumnsDefinition(cols =>
                    {
                        cols.ConstantColumn(22);
                        cols.RelativeColumn(3);
                        cols.RelativeColumn(2);
                        cols.RelativeColumn(2);
                        cols.RelativeColumn(2);
                        cols.RelativeColumn(2);
                        cols.RelativeColumn(2);
                    });

                    table.Header(header =>
                    {
                        var headerCells = new[] { "#", "Empleado", "Proyecto", "Sueldo bruto", "INFONAVIT", "Sueldo neto", "Pagado" };
                        foreach (var h in headerCells)
                            header.Cell().Background(ReporteEstilos.ColorPrimario)
                                .Padding(6).Text(h).FontSize(8).Bold().FontColor("#FFFFFF");
                    });

                    var numero = 1;
                    foreach (var d in _periodo.Detalles)
                    {
                        var bg = numero % 2 == 0 ? ReporteEstilos.ColorFondoTabla : "#FFFFFF";
                        table.Cell().Background(bg).Padding(5).Text(numero.ToString()).FontSize(8).FontColor(ReporteEstilos.ColorGris);
                        table.Cell().Background(bg).Padding(5).Text($"{d.EmpleadoNombre} ({d.EmpleadoNumero})").FontSize(8);
                        table.Cell().Background(bg).Padding(5).Text(d.ProyectoNombre ?? "Sin asignar").FontSize(8);
                        table.Cell().Background(bg).Padding(5).Text($"${d.SueldoBruto:N2}").FontSize(8);
                        table.Cell().Background(bg).Padding(5).Text($"${d.DescuentoInfonavit:N2}").FontSize(8);
                        table.Cell().Background(bg).Padding(5).Text($"${d.SueldoNeto:N2}").FontSize(8).Bold();
                        table.Cell().Background(bg).Padding(5).Text(d.Pagado ? "Sí" : "No").FontSize(8)
                            .FontColor(d.Pagado ? ReporteEstilos.ColorExito : ReporteEstilos.ColorAdvertencia);
                        numero++;
                    }

                    if (_periodo.Detalles.Count == 0)
                    {
                        table.Cell().ColumnSpan(7).Padding(12)
                            .Text("Sin empleados en este periodo.").FontSize(9)
                            .FontColor(ReporteEstilos.ColorGris).Italic();
                    }
                });

                col.Item().PaddingTop(16).Text(
                    "Este reporte se calcula sobre el sueldo neto semanal capturado por empleado, sin considerar asistencias ni horas trabajadas.")
                    .FontSize(8).Italic().FontColor(ReporteEstilos.ColorGris);
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
