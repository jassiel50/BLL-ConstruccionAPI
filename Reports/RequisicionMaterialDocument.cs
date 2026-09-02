using BLL_ConstruccionAPI.Models.Inventario.Proyectos;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace BLL_ConstruccionAPI.Reports;

/// <summary>
/// Requisición de materiales para solicitar al cliente/proveedor, replicando el formato
/// oficial "SOLICITUD DE MATERIALES" (encabezado de empresa, datos de la requisición,
/// tabla de partidas y firmas de solicitó/autorizó).
/// </summary>
public class RequisicionMaterialDocument : IDocument
{
    private const string RazonSocial     = "FRANCISCO SARABIA NO 126 COL NUEVA LIBERTAD";
    private const string Domicilio       = "GUADALUPE N.L.                         C.P. 67120";
    private const string RfcEmpresa      = "SPI240327CJA";
    private const string TelefonoCorreo  = "TEL (81) 2578 7691         blopez@bll.com.mx";
    private const string AutorizoNombre  = "ING BALDEMAR LOPEZ LOPEZ";

    private readonly RequisicionMaterial _req;

    public RequisicionMaterialDocument(RequisicionMaterial req)
    {
        _req = req;
    }

    public void Compose(IDocumentContainer container)
    {
        container.Page(page =>
        {
            page.Size(PageSizes.A4);
            page.Margin(30);
            page.DefaultTextStyle(t => t.FontSize(9.5f));
            page.Footer().Element(ReporteEstilos.AgregarPiePagina);

            page.Content().Column(col =>
            {
                col.Spacing(4);

                // ─── Encabezado de empresa ───────────────────────────────────
                col.Item().AlignCenter().Text("S O L I C I T U D   D E   M A T E R I A L E S")
                    .FontSize(14).Bold().FontColor(ReporteEstilos.ColorPrimario);
                col.Item().AlignCenter().Text(RazonSocial).FontSize(8.5f);
                col.Item().AlignCenter().Text($"{Domicilio}     RFC. {RfcEmpresa}").FontSize(8.5f);
                col.Item().AlignCenter().Text(TelefonoCorreo).FontSize(8.5f);

                col.Item().PaddingTop(10).LineHorizontal(1).LineColor(ReporteEstilos.ColorBordeTabla);

                // ─── Datos de la requisición ──────────────────────────────────
                col.Item().PaddingTop(8).Border(0.5f).BorderColor(ReporteEstilos.ColorBordeTabla).Padding(6)
                    .Text($"OBRA / PROYECTO: {_req.Proyecto?.Nombre}").FontSize(10).Bold();

                col.Item().Border(0.5f).BorderColor(ReporteEstilos.ColorBordeTabla).Padding(6)
                    .Text($"REQUISICIÓN DE MATERIALES No: {_req.Folio}").FontSize(10).Bold();

                col.Item().Row(r =>
                {
                    r.RelativeItem().Border(0.5f).BorderColor(ReporteEstilos.ColorBordeTabla).Padding(6)
                        .Text($"SE REQUIERE PARA: {_req.SeRequierePara}").FontSize(9.5f);
                    r.ConstantItem(150).Border(0.5f).BorderColor(ReporteEstilos.ColorBordeTabla).Padding(6)
                        .Text($"FECHA DE SOLICITUD: {_req.FechaSolicitud:dd/MM/yyyy}").FontSize(9.5f);
                });

                col.Item().Border(0.5f).BorderColor(ReporteEstilos.ColorBordeTabla).Padding(6)
                    .Text($"SE SUMINISTRA POR: {_req.SeSuministraPor}").FontSize(9.5f);

                col.Item().PaddingTop(10);

                // ─── Tabla de partidas ────────────────────────────────────────
                col.Item().Table(table =>
                {
                    table.ColumnsDefinition(c =>
                    {
                        c.ConstantColumn(45);   // PARTIDA
                        c.RelativeColumn(4);    // DESCRIPCIÓN
                        c.ConstantColumn(55);   // UNIDAD
                        c.ConstantColumn(55);   // CANTIDAD
                        c.RelativeColumn(3);    // ÁREA / COMENTARIOS
                        c.ConstantColumn(65);   // STATUS
                    });

                    void CeldaHeader(string texto) =>
                        table.Cell().Border(0.5f).BorderColor(ReporteEstilos.ColorBordeTabla)
                            .Background(ReporteEstilos.ColorPrimario).Padding(5)
                            .Text(texto).FontSize(8).Bold().FontColor(Colors.White);

                    CeldaHeader("PARTIDA");
                    CeldaHeader("DESCRIPCIÓN");
                    CeldaHeader("UNIDAD");
                    CeldaHeader("CANTIDAD");
                    CeldaHeader("ÁREA / COMENTARIOS");
                    CeldaHeader("STATUS");

                    void Celda(string texto, bool centrado = false)
                    {
                        var celda = table.Cell().Border(0.5f).BorderColor(ReporteEstilos.ColorBordeTabla).Padding(5);
                        var t = celda.Text(texto).FontSize(8.5f);
                        if (centrado) t.AlignCenter();
                    }

                    foreach (var d in _req.Detalles.OrderBy(d => d.Orden))
                    {
                        Celda(d.Orden.ToString(), centrado: true);
                        Celda(d.Descripcion);
                        Celda(d.Unidad, centrado: true);
                        Celda(d.Cantidad.ToString("0.##"), centrado: true);
                        Celda(d.AreaComentarios ?? string.Empty);
                        Celda(d.Status, centrado: true);
                    }
                });

                // ─── Firmas ────────────────────────────────────────────────────
                col.Item().PaddingTop(50).Row(r =>
                {
                    r.RelativeItem().Column(c =>
                    {
                        c.Item().AlignCenter().Text("SOLICITÓ").FontSize(9).Bold();
                        c.Item().PaddingTop(28).AlignCenter().LineHorizontal(0.75f).LineColor(ReporteEstilos.ColorGris);
                        c.Item().AlignCenter().Text(_req.SolicitoNombre).FontSize(8.5f);
                        c.Item().AlignCenter().Text("FIRMA").FontSize(7.5f).FontColor(ReporteEstilos.ColorGris);
                    });
                    r.ConstantItem(30);
                    r.RelativeItem().Column(c =>
                    {
                        c.Item().AlignCenter().Text("AUTORIZÓ").FontSize(9).Bold();
                        c.Item().PaddingTop(28).AlignCenter().LineHorizontal(0.75f).LineColor(ReporteEstilos.ColorGris);
                        c.Item().AlignCenter().Text(AutorizoNombre).FontSize(8.5f);
                        c.Item().AlignCenter().Text("FIRMA").FontSize(7.5f).FontColor(ReporteEstilos.ColorGris);
                    });
                });
            });
        });
    }
}
