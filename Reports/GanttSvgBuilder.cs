using System.Globalization;
using System.Text;
using BLL_ConstruccionAPI.DTOs.Fases;

namespace BLL_ConstruccionAPI.Reports;

/// <summary>
/// Genera el mismo diagrama de Gantt (SVG) que Pages/Proyectos/GanttProyecto.razor dibuja en el
/// navegador, para poder embeberlo tal cual en el reporte de avance para cliente vía QuestPDF.
/// Cualquier cambio visual al Gantt interactivo debe reflejarse aquí también.
/// </summary>
public static class GanttSvgBuilder
{
    private const int NameW = 220;
    private const int RowH = 48;
    private const int HeaderH = 50;
    private const int Pad = 16;

    private record PhaseBar(double BarX, double BarY, double BarW, double BarH,
        string Color, bool Completada, string FechaFin, string Nombre, double RowY);

    public static string Build(DateTime fechaInicioProyecto, List<FaseResponseDto> fases)
    {
        var fasesOrdenadas = fases.OrderBy(f => f.Orden).ToList();

        var fechaInicio = fechaInicioProyecto.Date;
        var fechaFin = fasesOrdenadas.Count > 0
            ? fasesOrdenadas.Max(f => f.FechaLimite).Date.AddDays(10)
            : fechaInicio.AddDays(30);

        var duracionProyecto = Math.Max(1, (fechaFin - fechaInicio).Days);
        var dayW = Math.Max(2.5, Math.Min(18.0, 900.0 / duracionProyecto));
        var svgW = NameW + (int)(duracionProyecto * dayW) + Pad * 2;
        var svgH = HeaderH + fasesOrdenadas.Count * RowH + Pad;

        var hoy = DateTime.UtcNow.Date;
        var hoyX = NameW + Pad + (hoy - fechaInicio).TotalDays * dayW;
        var mostrarHoy = hoy >= fechaInicio && hoy <= fechaFin;

        // Filas alternadas
        var rowBands = fasesOrdenadas
            .Select((_, i) => (Y: (double)(HeaderH + i * RowH), Fill: i % 2 == 0 ? "#ffffff" : "#f8fafc"))
            .ToList();

        // Marcadores de meses
        var monthMarkers = new List<(double X, string Label)>();
        var cursor = new DateTime(fechaInicio.Year, fechaInicio.Month, 1);
        while (cursor <= fechaFin)
        {
            var mx = NameW + Pad + (cursor - fechaInicio).TotalDays * dayW;
            if (mx >= NameW)
                monthMarkers.Add((mx, cursor.ToString("MMM yyyy", CultureInfo.GetCultureInfo("es-MX"))));
            cursor = cursor.AddMonths(1);
        }

        // Líneas de semanas (solo si hay zoom suficiente)
        var weekLines = new List<double>();
        if (dayW > 4)
        {
            var sem = fechaInicio.AddDays(-(int)fechaInicio.DayOfWeek + 1);
            while (sem <= fechaFin)
            {
                if (sem > fechaInicio)
                    weekLines.Add(NameW + Pad + (sem - fechaInicio).TotalDays * dayW);
                sem = sem.AddDays(7);
            }
        }

        // Barras de fases (bloques secuenciales: cada fase inicia donde termina la anterior)
        var phaseBars = new List<PhaseBar>();
        DateTime? anteriorFin = null;
        for (var i = 0; i < fasesOrdenadas.Count; i++)
        {
            var fase = fasesOrdenadas[i];
            var fIni = anteriorFin ?? fechaInicio;
            var fFin = fase.FechaLimite.Date;
            anteriorFin = fFin;

            var bX = NameW + Pad + (fIni - fechaInicio).TotalDays * dayW;
            var bW = Math.Max(dayW, (fFin - fIni).TotalDays * dayW);
            var bY = HeaderH + i * RowH + 10;
            var bH = RowH - 20;
            var rowY = HeaderH + i * RowH;

            var color = fase.Estado == "Completada" ? "#16a34a"
                : fase.Atrasada ? "#dc2626"
                : fase.PorVencer ? "#f59e0b"
                : "#1e40af";

            phaseBars.Add(new PhaseBar(bX, bY, bW, bH, color,
                fase.Estado == "Completada", fFin.ToString("dd/MM"), fase.Nombre, rowY));
        }

        return BuildSvg(svgW, svgH, dayW, rowBands, monthMarkers, weekLines, phaseBars, mostrarHoy, hoyX);
    }

    private static string F(double v) => v.ToString("F2", CultureInfo.InvariantCulture);

    private static string BuildSvg(int svgW, int svgH, double dayW,
        List<(double Y, string Fill)> rowBands,
        List<(double X, string Label)> monthMarkers,
        List<double> weekLines,
        List<PhaseBar> phaseBars,
        bool mostrarHoy, double hoyX)
    {
        var sb = new StringBuilder();
        sb.Append($"<svg width=\"{svgW}\" height=\"{svgH}\" viewBox=\"0 0 {svgW} {svgH}\" xmlns=\"http://www.w3.org/2000/svg\" style=\"font-family:'Inter',sans-serif;display:block;\">");

        foreach (var row in rowBands)
        {
            sb.Append($"<rect x=\"0\" y=\"{F(row.Y)}\" width=\"{svgW}\" height=\"{RowH}\" fill=\"{row.Fill}\"/>");
            sb.Append($"<line x1=\"0\" y1=\"{F(row.Y + RowH)}\" x2=\"{svgW}\" y2=\"{F(row.Y + RowH)}\" stroke=\"#f1f5f9\" stroke-width=\"1\"/>");
        }

        foreach (var m in monthMarkers)
            sb.Append($"<line x1=\"{F(m.X)}\" y1=\"0\" x2=\"{F(m.X)}\" y2=\"{svgH}\" stroke=\"#e2e8f0\" stroke-width=\"1\"/>");

        foreach (var sw in weekLines)
            sb.Append($"<line x1=\"{F(sw)}\" y1=\"{HeaderH}\" x2=\"{F(sw)}\" y2=\"{svgH}\" stroke=\"#f1f5f9\" stroke-width=\"1\" stroke-dasharray=\"3,3\"/>");

        sb.Append($"<rect x=\"0\" y=\"0\" width=\"{NameW}\" height=\"{svgH}\" fill=\"white\"/>");
        sb.Append($"<line x1=\"{NameW}\" y1=\"0\" x2=\"{NameW}\" y2=\"{svgH}\" stroke=\"#e2e8f0\" stroke-width=\"1.5\"/>");

        sb.Append($"<rect x=\"0\" y=\"0\" width=\"{svgW}\" height=\"{HeaderH}\" fill=\"#f8fafc\"/>");
        sb.Append($"<line x1=\"0\" y1=\"{HeaderH}\" x2=\"{svgW}\" y2=\"{HeaderH}\" stroke=\"#e2e8f0\" stroke-width=\"1.5\"/>");
        sb.Append("<text x=\"12\" y=\"30\" font-size=\"10\" fill=\"#64748b\" font-weight=\"700\">FASE</text>");

        foreach (var m in monthMarkers)
            sb.Append($"<text x=\"{F(m.X + 4)}\" y=\"30\" font-size=\"9\" fill=\"#94a3b8\" font-weight=\"600\">{m.Label}</text>");

        foreach (var bar in phaseBars)
        {
            sb.Append($"<rect x=\"{F(bar.BarX)}\" y=\"{F(bar.BarY)}\" width=\"{F(bar.BarW)}\" height=\"{F(bar.BarH)}\" rx=\"4\" fill=\"{bar.Color}\" opacity=\"0.88\"/>");
            if (bar.Completada)
                sb.Append($"<text x=\"{F(bar.BarX + 5)}\" y=\"{F(bar.BarY + bar.BarH / 2 + 4)}\" font-size=\"10\" fill=\"white\" font-weight=\"700\">✓</text>");
            if (dayW > 5)
                sb.Append($"<text x=\"{F(bar.BarX + bar.BarW + 3)}\" y=\"{F(bar.BarY + bar.BarH / 2 + 4)}\" font-size=\"9\" fill=\"#64748b\">{bar.FechaFin}</text>");

            var nombre = bar.Nombre.Length > 22 ? bar.Nombre[..22] + "…" : bar.Nombre;
            sb.Append($"<text x=\"12\" y=\"{F(bar.RowY + RowH / 2 + 4)}\" font-size=\"11\" fill=\"#002046\" font-weight=\"600\">{System.Net.WebUtility.HtmlEncode(nombre)}</text>");
        }

        if (mostrarHoy)
        {
            sb.Append($"<line x1=\"{F(hoyX)}\" y1=\"{HeaderH}\" x2=\"{F(hoyX)}\" y2=\"{svgH}\" stroke=\"#ef4444\" stroke-width=\"2\" stroke-dasharray=\"5,3\"/>");
            sb.Append($"<rect x=\"{F(hoyX - 16)}\" y=\"{HeaderH - 16}\" width=\"32\" height=\"14\" rx=\"3\" fill=\"#ef4444\"/>");
            sb.Append($"<text x=\"{F(hoyX)}\" y=\"{HeaderH - 5}\" font-size=\"8\" fill=\"white\" font-weight=\"700\" text-anchor=\"middle\">HOY</text>");
        }

        sb.Append("</svg>");
        return sb.ToString();
    }
}
