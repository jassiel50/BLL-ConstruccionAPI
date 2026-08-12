using BLL_ConstruccionAPI.Models.Servicios;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace BLL_ConstruccionAPI.Reports;

/// <summary>
/// Reporte de servicio firmado — replica el machote en papel usado en campo
/// (solicitud, datos del servicio, recursos, tipo de trabajo, evidencias y recepción).
/// </summary>
public class ReporteServicioDocument : IDocument
{
    // Value/Label de las disciplinas del machote — el Value debe coincidir
    // exactamente con lo que envía el checklist "Tipo de trabajo" del frontend.
    public static readonly (string Value, string Label)[] TiposTrabajoCatalogo =
    [
        ("Paileria", "Pailería"),
        ("Mecanica", "Mecánica"),
        ("Electrica", "Eléctrica"),
        ("Neumatica", "Neumática"),
        ("Hidraulica", "Hidráulica"),
        ("Electronica", "Electrónica"),
        ("Plomeria", "Plomería"),
        ("Albanileria", "Albañilería"),
        ("Supervision", "Supervisión"),
        ("Inspeccion", "Inspección"),
        ("MantenimientoPreventivo", "Mantenimiento Preventivo"),
        ("MantenimientoCorrectivo", "Mantenimiento Correctivo")
    ];

    private readonly Servicio _servicio;
    private readonly List<ServicioFoto> _fotos;

    public ReporteServicioDocument(Servicio servicio, List<ServicioFoto> fotos)
    {
        _servicio = servicio;
        _fotos = fotos;
    }

    private static string ValorOGuion(string? valor) => string.IsNullOrWhiteSpace(valor) ? "—" : valor;

    private string NombreCliente =>
        _servicio.ClienteId.HasValue && _servicio.Cliente is not null ? _servicio.Cliente.Nombre : _servicio.ClienteNombre;

    private static byte[]? DecodificarImagen(string? dataUri)
    {
        if (string.IsNullOrWhiteSpace(dataUri)) return null;
        var comaIndex = dataUri.IndexOf(',');
        var base64 = comaIndex >= 0 ? dataUri[(comaIndex + 1)..] : dataUri;
        try { return Convert.FromBase64String(base64); }
        catch { return null; }
    }

    public void Compose(IDocumentContainer container)
    {
        var tiposSeleccionados = (_servicio.TiposTrabajo ?? string.Empty)
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .ToHashSet();

        container.Page(page =>
        {
            page.Size(PageSizes.A4);
            page.Margin(35);
            page.DefaultTextStyle(t => t.FontSize(9.5f).FontFamily("Arial"));

            page.Header().Element(c => ReporteEstilos.AgregarEncabezado(c, "REPORTE DE SERVICIO",
                $"Folio #S-{_servicio.Id:D4}  ·  Fecha: {_servicio.FechaInicio:dd/MM/yyyy}"));

            page.Footer().Element(ReporteEstilos.AgregarPiePagina);

            page.Content().PaddingTop(14).Column(col =>
            {
                col.Spacing(10);

                // ── 1. Solicitud de Servicio ──────────────────────────────
                Seccion(col, "1. Solicitud de Servicio");
                col.Item().Text(ValorOGuion(_servicio.DescripcionTrabajo));

                col.Item().PaddingTop(4).Row(row =>
                {
                    row.RelativeItem().Text(t =>
                    {
                        t.Span("Solicitante: ").Bold();
                        t.Span(ValorOGuion(_servicio.NombreSolicitante));
                    });
                });

                var firmaSolicitante = DecodificarImagen(_servicio.FirmaSolicitanteBase64);
                if (firmaSolicitante is not null)
                    col.Item().PaddingTop(2).Width(180).Image(firmaSolicitante);
                col.Item().Text("Firma de quien solicita").FontSize(7.5f).FontColor(ReporteEstilos.ColorGris);

                // ── 2. Datos del Servicio ─────────────────────────────────
                Seccion(col, "2. Datos del Servicio");
                DatoFila(col, "Operador responsable", ValorOGuion(_servicio.OperadorNombre));
                var equipo = (_servicio.EquipoTrabajo ?? string.Empty)
                    .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                if (equipo.Length > 0)
                    DatoFila(col, "Equipo de trabajo", string.Join(", ", equipo));
                DatoFila(col, "Cliente", ValorOGuion(NombreCliente));
                DatoFila(col, "Dirección del servicio", ValorOGuion(_servicio.DireccionServicio));
                DatoFila(col, "Tipo de servicio", ValorOGuion(_servicio.Tipo.ToString()));
                DatoFila(col, "Máquina / Área", ValorOGuion(_servicio.Equipo));
                DatoFila(col, "Horario de trabajo", ValorOGuion(_servicio.HorarioTrabajo));
                DatoFila(col, "Número de trabajadores", _servicio.NumeroTrabajadores?.ToString() ?? "—");
                DatoFila(col, "Total de horas trabajadas", _servicio.TotalHorasTrabajadas?.ToString("0.##") ?? "—");

                // ── 3. Recursos Utilizados ────────────────────────────────
                Seccion(col, "3. Recursos Utilizados");
                DatoFila(col, "Mano de Obra", ValorOGuion(_servicio.RecursoManoDeObra));
                DatoFila(col, "Herramienta", ValorOGuion(_servicio.RecursoHerramienta));
                DatoFila(col, "Refacciones", ValorOGuion(_servicio.RecursoRefacciones));
                DatoFila(col, "Consumibles", ValorOGuion(_servicio.RecursoConsumibles));

                // ── 4. Tipo de Trabajo ─────────────────────────────────────
                Seccion(col, "4. Tipo de Trabajo");
                col.Item().Row(row =>
                {
                    foreach (var grupo in TiposTrabajoCatalogo.Chunk(4))
                    {
                        row.RelativeItem().Column(c =>
                        {
                            foreach (var (value, label) in grupo)
                            {
                                var marcado = tiposSeleccionados.Contains(value);
                                c.Item().Text(t =>
                                {
                                    t.Span(marcado ? "☑ " : "☐ ").FontSize(10);
                                    t.Span(label);
                                });
                            }
                        });
                    }
                });

                // ── 5. Evidencias ──────────────────────────────────────────
                Seccion(col, "5. Evidencias");
                if (_fotos.Count == 0)
                {
                    col.Item().Text("Sin evidencias.").FontColor(ReporteEstilos.ColorGris);
                }
                else
                {
                    foreach (var fila in _fotos.Chunk(3))
                    {
                        col.Item().Row(row =>
                        {
                            foreach (var foto in fila)
                                row.RelativeItem().Padding(2).Height(140).Image(foto.Contenido).FitArea();
                        });
                    }
                }

                // ── 6. Recepción del Servicio ─────────────────────────────
                Seccion(col, "6. Recepción del Servicio");
                col.Item().Text(t =>
                {
                    t.Span("Recibido por: ").Bold();
                    t.Span(ValorOGuion(_servicio.NombreQuienFirma));
                });

                var firmaRecepcion = DecodificarImagen(_servicio.FirmaBase64);
                if (firmaRecepcion is not null)
                    col.Item().PaddingTop(2).Width(180).Image(firmaRecepcion);
                col.Item().Text("Firma de recepción").FontSize(7.5f).FontColor(ReporteEstilos.ColorGris);
            });
        });
    }

    private static void Seccion(QuestPDF.Fluent.ColumnDescriptor col, string titulo)
    {
        col.Item().PaddingTop(6).Text(titulo).Bold().FontSize(11).FontColor(ReporteEstilos.ColorPrimario);
        col.Item().PaddingBottom(2).LineHorizontal(0.75f).LineColor(ReporteEstilos.ColorBordeTabla);
    }

    private static void DatoFila(QuestPDF.Fluent.ColumnDescriptor col, string etiqueta, string valor)
    {
        col.Item().Row(row =>
        {
            row.ConstantItem(160).Text(etiqueta).FontColor(ReporteEstilos.ColorGris);
            row.RelativeItem().Text(valor);
        });
    }
}
