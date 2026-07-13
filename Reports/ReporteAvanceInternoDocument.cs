using BLL_ConstruccionAPI.DTOs.Fases;
using BLL_ConstruccionAPI.DTOs.Pagos;
using BLL_ConstruccionAPI.DTOs.Proyectos;
using BLL_ConstruccionAPI.Models.Inventario.Proyectos;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace BLL_ConstruccionAPI.Reports;

/// <summary>
/// Reporte de avance para uso interno del equipo: incluye todo lo del reporte de
/// cliente (fases + estado de cuenta) más el resumen financiero completo
/// (presupuesto, gasto real desglosado, utilidad, varianza). No se debe enviar a clientes.
/// </summary>
public class ReporteAvanceInternoDocument : IDocument
{
    private readonly Proyecto _proyecto;
    private readonly ProyectoResponseDto _financiero;
    private readonly List<FaseResponseDto> _fases;
    private readonly ResumenPagosDto _pagos;

    public ReporteAvanceInternoDocument(Proyecto proyecto, ProyectoResponseDto financiero, List<FaseResponseDto> fases, ResumenPagosDto pagos)
    {
        _proyecto = proyecto;
        _financiero = financiero;
        _fases = fases;
        _pagos = pagos;
    }

    public void Compose(IDocumentContainer container)
    {
        var totalFases = _fases.Count;
        var completadas = _fases.Count(f => f.Estado == "Completada");
        var atrasadas = _fases.Count(f => f.Atrasada);
        var porcentaje = totalFases > 0 ? (double)completadas / totalFases * 100 : 0;
        var diasTranscurridos = Math.Max(0, (DateTime.UtcNow.Date - _proyecto.FechaInicio.Date).Days);

        container.Page(page =>
        {
            page.Size(PageSizes.A4);
            page.Margin(30);
            page.Header().Element(c => ReporteEstilos.AgregarEncabezado(c,
                "Reporte de Avance y Estado Financiero",
                $"Proyecto: {_proyecto.Nombre} — Uso interno"));
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
                    Etiqueta("Fecha Fin Est.:"); Valor(_proyecto.FechaFin?.ToString("dd/MM/yyyy") ?? "Por definir");
                    Etiqueta("Estado:"); Valor(_proyecto.Estado.ToString());
                    Etiqueta("Días transcurridos:"); Valor($"{diasTranscurridos} día(s)");
                });

                col.Item().PaddingTop(16);

                // ─── Avance general ───────────────────────────────────────
                col.Item().Row(row =>
                {
                    InfoBox(row.RelativeItem(), "Avance General", $"{porcentaje:N0}%", ReporteEstilos.ColorPrimario);
                    row.ConstantItem(8);
                    InfoBox(row.RelativeItem(), "Fases Completadas", $"{completadas} de {totalFases}", ReporteEstilos.ColorExito);
                    row.ConstantItem(8);
                    InfoBox(row.RelativeItem(), "Fases Atrasadas", atrasadas.ToString(),
                        atrasadas > 0 ? ReporteEstilos.ColorAlerta : ReporteEstilos.ColorGris);
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
                        c.RelativeColumn(4);
                        c.ConstantColumn(75);
                        c.ConstantColumn(95);
                    });

                    table.Header(h =>
                    {
                        foreach (var t in new[] { "#", "Fase", "Descripción", "Fecha Límite", "Estado" })
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
                        table.Cell().Background(bg).Padding(5).Text(fase.FechaLimite.ToString("dd/MM/yyyy")).FontSize(8);
                        table.Cell().Background(bg).Padding(5).Text(estadoTexto).FontSize(8).Bold().FontColor(estadoColor);
                        num++;
                    }

                    if (_fases.Count == 0)
                    {
                        table.Cell().ColumnSpan(5).Padding(12)
                            .Text("Sin fases registradas.").FontSize(9).FontColor(ReporteEstilos.ColorGris).Italic();
                    }
                });

                col.Item().PaddingTop(20);

                // ─── Resumen financiero interno ───────────────────────────
                col.Item().Text("Resumen Financiero (interno)").FontSize(11).Bold().FontColor(ReporteEstilos.ColorPrimario);
                col.Item().PaddingTop(8).Row(row =>
                {
                    InfoBox(row.RelativeItem(), "Monto Contrato", $"${_financiero.MontoContrato:N2}", ReporteEstilos.ColorPrimario);
                    row.ConstantItem(8);
                    InfoBox(row.RelativeItem(), "Presupuesto Est.", $"${_financiero.PresupuestoEstimado:N2}", ReporteEstilos.ColorPrimario);
                    row.ConstantItem(8);
                    InfoBox(row.RelativeItem(), "Gasto Real", $"${_financiero.GastoReal:N2}", ReporteEstilos.ColorAdvertencia);
                    row.ConstantItem(8);
                    InfoBox(row.RelativeItem(), "Utilidad", $"${_financiero.Utilidad:N2}",
                        _financiero.Utilidad >= 0 ? ReporteEstilos.ColorExito : ReporteEstilos.ColorAlerta);
                });

                col.Item().PaddingTop(10).Table(table =>
                {
                    table.ColumnsDefinition(c =>
                    {
                        c.RelativeColumn();
                        c.RelativeColumn();
                        c.RelativeColumn();
                    });

                    table.Header(h =>
                    {
                        foreach (var t in new[] { "Materiales", "Herramientas", "Extras" })
                            h.Cell().Background(ReporteEstilos.ColorSecundario).Padding(5)
                                .Text(t).FontSize(8).Bold().FontColor("#FFFFFF");
                    });

                    table.Cell().Background(ReporteEstilos.ColorFondoTabla).Padding(6).Text($"${_financiero.GastoMateriales:N2}").FontSize(9);
                    table.Cell().Background(ReporteEstilos.ColorFondoTabla).Padding(6).Text($"${_financiero.GastoHerramientas:N2}").FontSize(9);
                    table.Cell().Background(ReporteEstilos.ColorFondoTabla).Padding(6).Text($"${_financiero.GastoExtras:N2}").FontSize(9);
                });

                if (_financiero.SobrepasadoPresupuesto || _financiero.SobrepasadoContrato)
                {
                    col.Item().PaddingTop(10).Background("#ffdad6").Padding(8).Text(t =>
                    {
                        if (_financiero.SobrepasadoPresupuesto)
                            t.Span("⚠ El gasto real supera el presupuesto estimado.  ").FontSize(9).FontColor(ReporteEstilos.ColorAlerta).Bold();
                        if (_financiero.SobrepasadoContrato)
                            t.Span("⚠ El gasto real supera el monto del contrato.").FontSize(9).FontColor(ReporteEstilos.ColorAlerta).Bold();
                    });
                }

                if (_financiero.PresupuestoEstimado > 0)
                {
                    col.Item().PaddingTop(10).Text(
                        $"Varianza contra presupuesto: {(_financiero.Varianza >= 0 ? "+" : "")}${_financiero.Varianza:N2}")
                        .FontSize(9).Italic().FontColor(ReporteEstilos.ColorGris);
                }

                col.Item().PaddingTop(20);

                // ─── Estado de cuenta ─────────────────────────────────────
                col.Item().Text("Estado de Cuenta").FontSize(11).Bold().FontColor(ReporteEstilos.ColorPrimario);
                col.Item().PaddingTop(8).Row(row =>
                {
                    InfoBox(row.RelativeItem(), "Monto Contrato", $"${_pagos.MontoContrato:N2}", ReporteEstilos.ColorPrimario);
                    row.ConstantItem(8);
                    InfoBox(row.RelativeItem(), "Total Pagado", $"${_pagos.TotalPagado:N2}", ReporteEstilos.ColorExito);
                    row.ConstantItem(8);
                    var colorSaldo = _pagos.SaldoPendiente <= 0 ? ReporteEstilos.ColorExito : ReporteEstilos.ColorAdvertencia;
                    InfoBox(row.RelativeItem(), "Saldo Pendiente", $"${_pagos.SaldoPendiente:N2}", colorSaldo);
                });

                if (_pagos.Pagos.Count > 0)
                {
                    col.Item().PaddingTop(12).Table(table =>
                    {
                        table.ColumnsDefinition(c =>
                        {
                            c.RelativeColumn(3);
                            c.RelativeColumn(2);
                            c.RelativeColumn(2);
                            c.RelativeColumn(2);
                        });

                        table.Header(h =>
                        {
                            foreach (var t in new[] { "Concepto", "Fecha", "Monto Pagado", "Estado" })
                                h.Cell().Background(ReporteEstilos.ColorSecundario).Padding(5)
                                    .Text(t).FontSize(8).Bold().FontColor("#FFFFFF");
                        });

                        var n = 1;
                        foreach (var pago in _pagos.Pagos.OrderByDescending(p => p.FechaPago))
                        {
                            var bg = n % 2 == 0 ? ReporteEstilos.ColorFondoTabla : "#FFFFFF";
                            table.Cell().Background(bg).Padding(5).Text(pago.Concepto).FontSize(8);
                            table.Cell().Background(bg).Padding(5).Text(pago.FechaPago.ToString("dd/MM/yyyy")).FontSize(8);
                            table.Cell().Background(bg).Padding(5).Text($"${pago.Monto:N2}").FontSize(8);
                            table.Cell().Background(bg).Padding(5).Text(pago.Estado).FontSize(8);
                            n++;
                        }
                    });

                    if (_pagos.MontoContrato > 0)
                    {
                        var pct = _pagos.TotalPagado / _pagos.MontoContrato * 100;
                        col.Item().PaddingTop(10).Text($"Porcentaje cubierto: {pct:N1}% del contrato")
                            .FontSize(9).Italic().FontColor(ReporteEstilos.ColorGris);
                    }
                }
                else
                {
                    col.Item().PaddingTop(10).Text("Sin pagos registrados.")
                        .FontSize(9).FontColor(ReporteEstilos.ColorGris).Italic();
                }
            });
        });
    }

    private static (string Texto, string Color) EstadoVisual(FaseResponseDto f)
    {
        if (f.Estado == "Completada")
            return f.CompletadaConRetraso
                ? ("Completada (con retraso)", ReporteEstilos.ColorAdvertencia)
                : ("Completada", ReporteEstilos.ColorExito);
        if (f.Atrasada) return ("Atrasada", ReporteEstilos.ColorAlerta);
        if (f.PorVencer) return ("Por vencer", ReporteEstilos.ColorAdvertencia);
        return ("En curso", ReporteEstilos.ColorGris);
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
