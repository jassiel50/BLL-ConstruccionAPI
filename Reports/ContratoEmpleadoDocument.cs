using BLL_ConstruccionAPI.Models.Personal;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace BLL_ConstruccionAPI.Reports;

/// <summary>
/// Contrato individual de trabajo por tiempo determinado — BORRADOR ESTÁNDAR.
/// Este documento es una plantilla genérica pensada como punto de partida y
/// DEBE ser revisada por un abogado laboral antes de utilizarse formalmente.
/// No constituye asesoría legal.
/// </summary>
public class ContratoEmpleadoDocument : IDocument
{
    // ─── Datos fijos de la empresa (patrón) ─────────────────────────────────
    private const string RazonSocial      = "SERVICIOS Y PROYECTOS INDUSTRIALES BLL";
    private const string RfcEmpresa       = "SPI240327CJA";
    private const string DomicilioEmpresa = "Francisco Sarabia 126, Colonia Nueva Libertad, Municipio Guadalupe, Nuevo León, C.P. 67120";
    private const string RepresentanteNombre = "Ing. Baldemar López";
    private const string RepresentantePuesto = "Director General";
    private const string CiudadFirma      = "Guadalupe, Nuevo León";

    private readonly Empleado _empleado;
    private readonly DateTime _fechaInicio;
    private readonly DateTime _fechaFin;
    private readonly int _duracionMeses;

    public ContratoEmpleadoDocument(Empleado empleado, DateTime fechaInicio, int duracionMeses)
    {
        _empleado = empleado;
        _fechaInicio = fechaInicio;
        _duracionMeses = duracionMeses <= 0 ? 3 : duracionMeses;
        _fechaFin = fechaInicio.AddMonths(_duracionMeses).AddDays(-1);
    }

    private static string ValorOGuion(string? valor) => string.IsNullOrWhiteSpace(valor) ? "____________________" : valor;

    public void Compose(IDocumentContainer container)
    {
        container.Page(page =>
        {
            page.Size(PageSizes.A4);
            page.Margin(35);
            page.DefaultTextStyle(t => t.FontSize(9.5f).FontFamily("Arial"));

            page.Header().Column(col =>
            {
                col.Item().Background("#fef3c7").Padding(6).Row(row =>
                {
                    row.AutoItem().PaddingRight(6).Text("⚠").FontSize(11);
                    row.RelativeItem().Text(
                        "BORRADOR — Plantilla estándar generada por el sistema. Debe ser revisada por un abogado laboral antes de firmarse. No constituye asesoría legal.")
                        .FontSize(8).FontColor("#92400e").Bold();
                });
                col.Item().PaddingTop(10).Column(c =>
                {
                    c.Item().Text(RazonSocial).FontSize(9).FontColor(ReporteEstilos.ColorGris).Bold();
                    c.Item().Text("CONTRATO INDIVIDUAL DE TRABAJO POR TIEMPO DETERMINADO")
                        .FontSize(14).FontColor(ReporteEstilos.ColorPrimario).Bold();
                });
                col.Item().PaddingTop(6).LineHorizontal(1.5f).LineColor(ReporteEstilos.ColorPrimario);
            });

            page.Footer().Element(ReporteEstilos.AgregarPiePagina);

            page.Content().PaddingTop(14).Column(col =>
            {
                col.Spacing(8);

                col.Item().Text(t =>
                {
                    t.Span("Contrato individual de trabajo por tiempo determinado que celebran, por una parte, ");
                    t.Span(RazonSocial).Bold();
                    t.Span($", con Registro Federal de Contribuyentes {RfcEmpresa}, con domicilio en {DomicilioEmpresa}, representada en este acto por ");
                    t.Span(RepresentanteNombre).Bold();
                    t.Span($" en su carácter de {RepresentantePuesto}, a quien en lo sucesivo se le denominará \"EL PATRÓN\"; y por la otra parte, el(la) C. ");
                    t.Span(_empleado.NombreCompleto).Bold();
                    t.Span(", a quien en lo sucesivo se le denominará \"EL TRABAJADOR\"; ambas partes sujetándose a las siguientes declaraciones y cláusulas:");
                });

                col.Item().PaddingTop(4).Text("DECLARACIONES").Bold().FontColor(ReporteEstilos.ColorPrimario);
                col.Item().Text(t =>
                {
                    t.Span("I. Declara \"EL PATRÓN\", por conducto de su representante, que es una persona moral legalmente constituida conforme a las leyes mexicanas, dedicada a la prestación de servicios y ejecución de proyectos industriales, y que cuenta con los recursos necesarios para la relación laboral objeto de este contrato.");
                });
                col.Item().Text(t =>
                {
                    t.Span("II. Declara \"EL TRABAJADOR\" ser una persona física con capacidad legal para obligarse en los términos del presente contrato, contar con CURP ");
                    t.Span(ValorOGuion(_empleado.CURP)).Bold();
                    t.Span(", RFC ");
                    t.Span(ValorOGuion(_empleado.RFC)).Bold();
                    t.Span(" y número de seguridad social (IMSS) ");
                    t.Span(ValorOGuion(_empleado.NSS)).Bold();
                    t.Span(", con domicilio en ");
                    t.Span(ValorOGuion(_empleado.Domicilio)).Bold();
                    t.Span(", y manifiesta su voluntad de prestar sus servicios personales subordinados a favor de \"EL PATRÓN\".");
                });

                col.Item().PaddingTop(4).Text("CLÁUSULAS").Bold().FontColor(ReporteEstilos.ColorPrimario);

                Clausula(col, "PRIMERA. OBJETO Y DURACIÓN.", t =>
                {
                    t.Span("\"EL PATRÓN\" contrata los servicios personales subordinados de \"EL TRABAJADOR\" para desempeñar el puesto de ");
                    t.Span(_empleado.Puesto).Bold();
                    t.Span(", por tiempo determinado, con fundamento en los artículos 35 y 37 de la Ley Federal del Trabajo, en virtud de la naturaleza temporal de las obras y proyectos a los que será asignado. El presente contrato tendrá vigencia del ");
                    t.Span(_fechaInicio.ToString("dd \\d\\e MMMM \\d\\e yyyy", System.Globalization.CultureInfo.GetCultureInfo("es-MX"))).Bold();
                    t.Span(" al ");
                    t.Span(_fechaFin.ToString("dd \\d\\e MMMM \\d\\e yyyy", System.Globalization.CultureInfo.GetCultureInfo("es-MX"))).Bold();
                    t.Span($" ({_duracionMeses} mes(es)), fecha en la que concluirá automáticamente sin necesidad de aviso previo, salvo que las partes acuerden por escrito su renovación.");
                });

                Clausula(col, "SEGUNDA. LUGAR Y NATURALEZA DEL TRABAJO.", t =>
                {
                    t.Span("\"EL TRABAJADOR\" prestará sus servicios en las obras, proyectos y ubicaciones que \"EL PATRÓN\" le asigne dentro del territorio nacional, conforme a las necesidades operativas de la empresa, pudiendo ser reasignado de un proyecto a otro sin que ello altere la naturaleza del presente contrato.");
                });

                Clausula(col, "TERCERA. JORNADA.", t =>
                {
                    t.Span("\"EL TRABAJADOR\" prestará sus servicios en la jornada y horario que determine \"EL PATRÓN\" conforme a las necesidades de cada obra o proyecto, respetando en todo momento los límites de jornada máxima y los descansos establecidos en la Ley Federal del Trabajo.");
                });

                Clausula(col, "CUARTA. SALARIO.", t =>
                {
                    t.Span("\"EL TRABAJADOR\" percibirá un salario neto semanal de ");
                    t.Span(_empleado.SueldoNetoSemanal.HasValue ? $"${_empleado.SueldoNetoSemanal.Value:N2} (M.N.)" : "____________________").Bold();
                    t.Span(", pagadero de forma semanal, sujeto a las retenciones y deducciones que conforme a la ley correspondan (incluyendo, en su caso, el descuento por crédito INFONAVIT que \"EL TRABAJADOR\" tenga vigente).");
                });

                Clausula(col, "QUINTA. PRESTACIONES DE LEY.", t =>
                {
                    t.Span("\"EL TRABAJADOR\" gozará de las prestaciones mínimas establecidas en la Ley Federal del Trabajo, incluyendo de manera enunciativa y no limitativa: aguinaldo, vacaciones y prima vacacional conforme a su antigüedad, así como afiliación al Instituto Mexicano del Seguro Social (IMSS).");
                });

                Clausula(col, "SEXTA. OBLIGACIONES DEL TRABAJADOR.", t =>
                {
                    t.Span("\"EL TRABAJADOR\" se obliga a desempeñar sus labores con el cuidado, diligencia y esmero apropiados, siguiendo las instrucciones de sus superiores, cumpliendo el Reglamento Interior de Trabajo y las normas de seguridad e higiene aplicables en cada obra o proyecto.");
                });

                Clausula(col, "SÉPTIMA. TERMINACIÓN.", t =>
                {
                    t.Span("El presente contrato terminará automáticamente al concluir el plazo pactado en la cláusula PRIMERA, sin responsabilidad para ninguna de las partes. Asimismo, podrá darse por terminado de forma anticipada por cualquiera de las causas previstas en el artículo 47 de la Ley Federal del Trabajo.");
                });

                Clausula(col, "OCTAVA. MANIFESTACIÓN.", t =>
                {
                    t.Span("Ambas partes manifiestan que el presente contrato se firma sin que medie dolo, error, mala fe ni ningún otro vicio del consentimiento, por lo que lo ratifican en todas y cada una de sus partes, firmándolo en la ciudad de ");
                    t.Span(CiudadFirma).Bold();
                    t.Span(", a la fecha de su firma.");
                });

                col.Item().PaddingTop(28).Row(row =>
                {
                    FirmaBox(row.RelativeItem(), "EL PATRÓN", RazonSocial, RepresentanteNombre);
                    row.ConstantItem(30);
                    FirmaBox(row.RelativeItem(), "EL TRABAJADOR", _empleado.NombreCompleto, _empleado.Puesto);
                });
            });
        });
    }

    private static void Clausula(QuestPDF.Fluent.ColumnDescriptor col, string titulo, Action<QuestPDF.Fluent.TextDescriptor> cuerpo)
    {
        col.Item().Text(t =>
        {
            t.Span(titulo + " ").Bold();
            cuerpo(t);
        });
    }

    private static void FirmaBox(IContainer container, string rol, string nombre, string detalle)
    {
        container.Column(c =>
        {
            c.Item().PaddingBottom(30).LineHorizontal(1).LineColor(ReporteEstilos.ColorGris);
            c.Item().AlignCenter().Text(rol).Bold().FontSize(9);
            c.Item().AlignCenter().Text(nombre).FontSize(9);
            c.Item().AlignCenter().Text(detalle).FontSize(8).FontColor(ReporteEstilos.ColorGris);
        });
    }
}
