using BLL_ConstruccionAPI.Models.Cotizaciones;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace BLL_ConstruccionAPI.Reports;

/// <summary>
/// Cotización de servicio para el cliente: datos del emisor, alcance, tabla de
/// partidas con subtotal/IVA/total, cláusulas, condiciones y firma.
/// </summary>
public class CotizacionDocument : IDocument
{
    private const string RazonSocial      = "SERVICIOS Y PROYECTOS INDUSTRIALES BLL, S.A. DE C.V.";
    private const string RfcEmpresa       = "SPI240327CJA";
    private const string DomicilioEmpresa = "Av. Francisco Sarabia 126, Col. Nueva Libertad, Guadalupe, N.L., C.P. 67120";
    private const string TelefonoEmpresa  = "81 2578 7691";
    private const string CorreoEmpresa    = "balde.mar@live.com.mx";
    private const string FirmanteNombre   = "BALDEMAR LÓPEZ LÓPEZ";

    private readonly Cotizacion _cot;
    private readonly string _empresaTexto;

    public CotizacionDocument(Cotizacion cot, string empresaTexto)
    {
        _cot = cot;
        _empresaTexto = empresaTexto;
    }

    public void Compose(IDocumentContainer container)
    {
        container.Page(page =>
        {
            page.Size(PageSizes.A4);
            page.Margin(30);
            page.DefaultTextStyle(t => t.FontSize(9.5f));
            page.Header().Element(c => ReporteEstilos.AgregarEncabezado(c, _cot.Titulo, $"Folio: {_cot.Folio}", conLogo: true));
            page.Footer().Element(ReporteEstilos.AgregarPiePagina);

            page.Content().PaddingTop(16).Column(col =>
            {
                col.Spacing(4);

                // ─── Datos del emisor ──────────────────────────────────────
                col.Item().Text(RazonSocial).FontSize(8).FontColor(ReporteEstilos.ColorGris).Bold();
                col.Item().Text($"{DomicilioEmpresa}  |  Tel: {TelefonoEmpresa}  |  RFC: {RfcEmpresa}  |  {CorreoEmpresa}")
                    .FontSize(7.5f).FontColor(ReporteEstilos.ColorGris);

                col.Item().PaddingTop(12);

                // ─── Datos de cotización / cliente ─────────────────────────
                col.Item().Border(0.5f).BorderColor(ReporteEstilos.ColorBordeTabla).Table(table =>
                {
                    table.ColumnsDefinition(c =>
                    {
                        c.ConstantColumn(90);
                        c.RelativeColumn();
                        c.ConstantColumn(90);
                        c.RelativeColumn();
                    });

                    void Etiqueta(string texto) =>
                        table.Cell().Background(ReporteEstilos.ColorSecundario).Padding(6)
                            .Text(texto).FontSize(8).FontColor(Colors.White).Bold();
                    void Valor(string texto) =>
                        table.Cell().Background(Colors.White).Padding(6).Text(texto).FontSize(9);

                    Etiqueta("Empresa:"); Valor(_empresaTexto);
                    Etiqueta("Atención:"); Valor(string.IsNullOrWhiteSpace(_cot.ContactoNombre) ? "-" : _cot.ContactoNombre!);
                    Etiqueta("Folio:"); Valor(_cot.Folio);
                    Etiqueta("Fecha:"); Valor(_cot.FechaCotizacion.ToString("dd/MM/yyyy"));
                });

                col.Item().PaddingTop(14);

                if (!string.IsNullOrWhiteSpace(_cot.Introduccion))
                    col.Item().Text(_cot.Introduccion).FontSize(9.5f);

                if (!string.IsNullOrWhiteSpace(_cot.AlcanceGeneral))
                {
                    col.Item().PaddingTop(10).Text("Alcance general:").FontSize(10).Bold().FontColor(ReporteEstilos.ColorPrimario);
                    col.Item().PaddingTop(4).Column(items =>
                    {
                        foreach (var linea in DividirLineas(_cot.AlcanceGeneral))
                            items.Item().Row(r =>
                            {
                                r.ConstantItem(12).Text("•").FontColor(ReporteEstilos.ColorPrimario);
                                r.RelativeItem().Text(linea).FontSize(9);
                            });
                    });
                }

                col.Item().PaddingTop(16);

                // ─── Tabla de partidas ──────────────────────────────────────
                col.Item().Table(table =>
                {
                    table.ColumnsDefinition(c =>
                    {
                        c.ConstantColumn(28);
                        c.RelativeColumn(6);
                        c.ConstantColumn(55);
                        c.ConstantColumn(55);
                        c.ConstantColumn(85);
                    });

                    table.Header(h =>
                    {
                        foreach (var t in new[] { "PT", "DESCRIPCIÓN", "CANT", "UNIDAD", "TOTAL" })
                            h.Cell().Background(ReporteEstilos.ColorPrimario).Padding(6)
                                .Text(t).FontSize(8).Bold().FontColor("#FFFFFF");
                    });

                    var num = 1;
                    foreach (var item in _cot.Items.OrderBy(i => i.Orden))
                    {
                        var bg = num % 2 == 0 ? ReporteEstilos.ColorFondoTabla : "#FFFFFF";
                        table.Cell().Background(bg).Padding(5).Text(num.ToString()).FontSize(8).FontColor(ReporteEstilos.ColorGris);
                        table.Cell().Background(bg).Padding(5).Text(item.Descripcion).FontSize(8);
                        table.Cell().Background(bg).Padding(5).Text(item.Cantidad).FontSize(8);
                        table.Cell().Background(bg).Padding(5).Text(item.Unidad).FontSize(8);
                        table.Cell().Background(bg).Padding(5).AlignRight().Text($"${item.Total:N2}").FontSize(8).Bold();
                        num++;
                    }
                });

                col.Item().PaddingTop(10).AlignRight().Column(c =>
                {
                    c.Item().Text($"SUBTOTAL: ${_cot.Subtotal:N2}").FontSize(9.5f);
                    c.Item().Text($"IVA: ${_cot.Iva:N2}").FontSize(9.5f);
                    c.Item().PaddingTop(2).Text($"TOTAL: ${_cot.Total:N2}").FontSize(12).Bold().FontColor(ReporteEstilos.ColorPrimario);
                });

                if (_cot.TiempoEntregaDias.HasValue || !string.IsNullOrWhiteSpace(_cot.Clausulas))
                {
                    col.Item().PaddingTop(18).Text("Cláusulas:").FontSize(10).Bold().FontColor(ReporteEstilos.ColorPrimario);
                    col.Item().PaddingTop(4).Column(items =>
                    {
                        if (_cot.TiempoEntregaDias.HasValue)
                            items.Item().Row(r =>
                            {
                                r.ConstantItem(12).Text("•").FontColor(ReporteEstilos.ColorPrimario);
                                r.RelativeItem().Text($"Tiempo de entrega {_cot.TiempoEntregaDias} día(s)").FontSize(9);
                            });
                        foreach (var linea in DividirLineas(_cot.Clausulas))
                            items.Item().Row(r =>
                            {
                                r.ConstantItem(12).Text("•").FontColor(ReporteEstilos.ColorPrimario);
                                r.RelativeItem().Text(linea).FontSize(9);
                            });
                    });
                }

                col.Item().PaddingTop(16).Text("Condiciones:").FontSize(10).Bold().FontColor(ReporteEstilos.ColorPrimario);
                col.Item().PaddingTop(4).Text($"Cotización válida por {_cot.ValidezDias} día(s)").FontSize(9);
                col.Item().Text($"Condiciones de pago: {_cot.CondicionesPago}").FontSize(9);
                col.Item().Text($"Método de pago: {_cot.MetodoPago}").FontSize(9);
                col.Item().PaddingTop(4).Text("Quedo al pendiente para su validación, dudas y aclaraciones favor de comunicarse.").FontSize(9);

                col.Item().PaddingTop(30).Text(FirmanteNombre).FontSize(10).Bold();
                col.Item().Text($"Cel. +52 {TelefonoEmpresa}").FontSize(8.5f).FontColor(ReporteEstilos.ColorGris);
                col.Item().Text(CorreoEmpresa).FontSize(8.5f).FontColor(ReporteEstilos.ColorGris);
            });
        });
    }

    private static IEnumerable<string> DividirLineas(string? texto) =>
        (texto ?? string.Empty)
            .Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Where(l => l.Length > 0);
}
