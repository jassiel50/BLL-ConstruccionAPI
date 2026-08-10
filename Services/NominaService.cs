using System.Security.Claims;
using BLL_ConstruccionAPI.Data;
using BLL_ConstruccionAPI.DTOs.Nomina;
using BLL_ConstruccionAPI.Models.Enums;
using BLL_ConstruccionAPI.Models.Nomina;
using BLL_ConstruccionAPI.Reports;
using BLL_ConstruccionAPI.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Fluent;

namespace BLL_ConstruccionAPI.Services;

public class NominaService : INominaService
{
    private const decimal TarifaHoraExtra = 100m;

    private readonly AppDbContext _context;
    private readonly IBitacoraService _bitacora;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IGastoSemanalService _gastoSemanalService;

    public NominaService(AppDbContext context, IBitacoraService bitacora, IHttpContextAccessor httpContextAccessor,
        IGastoSemanalService gastoSemanalService)
    {
        _context = context;
        _bitacora = bitacora;
        _httpContextAccessor = httpContextAccessor;
        _gastoSemanalService = gastoSemanalService;
    }

    private (int Id, string Nombre) GetUsuarioInfo()
    {
        var user = _httpContextAccessor.HttpContext?.User;
        var id = int.TryParse(user?.FindFirstValue(ClaimTypes.NameIdentifier), out var parsed) ? parsed : 0;
        var nombre = user?.FindFirstValue("nombreUsuario") ?? "Sistema";
        return (id, nombre);
    }

    private static PeriodoNominaDto ToDto(PeriodoNomina p, bool incluirDetalles)
    {
        var detalles = p.Detalles
            .OrderBy(d => d.Empleado?.NombreCompleto)
            .Select(d => new NominaDetalleDto
            {
                Id = d.Id,
                EmpleadoId = d.EmpleadoId,
                EmpleadoNombre = d.Empleado?.NombreCompleto ?? string.Empty,
                EmpleadoNumero = d.Empleado?.NumeroEmpleado ?? string.Empty,
                ProyectoId = d.ProyectoId,
                ProyectoNombre = d.Proyecto?.Nombre,
                SueldoDiario = d.SueldoDiario,
                DiasTrabajados = d.DiasTrabajados,
                Faltas = d.Faltas,
                Retardos = d.Retardos,
                HorasExtra = d.HorasExtra,
                SueldoBruto = d.SueldoBruto,
                DescuentoInfonavit = d.DescuentoInfonavit,
                MontoAjuste = d.MontoAjuste,
                MotivoAjuste = d.MotivoAjuste,
                SueldoNeto = d.SueldoNeto,
                Pagado = d.Pagado,
                FechaPago = d.FechaPago
            })
            .ToList();

        return new PeriodoNominaDto
        {
            Id = p.Id,
            FechaInicio = p.FechaInicio,
            FechaFin = p.FechaFin,
            FechaGeneracion = p.FechaGeneracion,
            Estado = p.Estado.ToString(),
            TotalEmpleados = detalles.Count,
            TotalBruto = detalles.Sum(d => d.SueldoBruto),
            TotalDescuentos = detalles.Sum(d => d.DescuentoInfonavit),
            TotalNeto = detalles.Sum(d => d.SueldoNeto),
            Detalles = incluirDetalles ? detalles : []
        };
    }

    public async Task<List<PeriodoNominaDto>> GetPeriodosAsync()
    {
        var periodos = await _context.PeriodosNomina
            .AsNoTracking()
            .Include(p => p.Detalles).ThenInclude(d => d.Empleado)
            .Include(p => p.Detalles).ThenInclude(d => d.Proyecto)
            .OrderByDescending(p => p.FechaInicio)
            .ToListAsync();

        return periodos.Select(p => ToDto(p, incluirDetalles: false)).ToList();
    }

    public async Task<PeriodoNominaDto?> GetPeriodoByIdAsync(int id)
    {
        var periodo = await _context.PeriodosNomina
            .AsNoTracking()
            .Include(p => p.Detalles).ThenInclude(d => d.Empleado)
            .Include(p => p.Detalles).ThenInclude(d => d.Proyecto)
            .FirstOrDefaultAsync(p => p.Id == id);

        return periodo is null ? null : ToDto(periodo, incluirDetalles: true);
    }

    public async Task<(bool Success, string Message, PeriodoNominaDto? Data)> GenerarPeriodoAsync(GenerarPeriodoNominaRequestDto dto, int usuarioId)
    {
        if (dto.FechaFin.Date < dto.FechaInicio.Date)
            return (false, "La fecha de fin no puede ser anterior a la fecha de inicio.", null);

        var traslape = await _context.PeriodosNomina.AnyAsync(p =>
            p.FechaInicio.Date <= dto.FechaFin.Date && p.FechaFin.Date >= dto.FechaInicio.Date);
        if (traslape)
            return (false, "Ya existe un periodo de nómina generado que se traslapa con estas fechas.", null);

        var empleadosActivos = await _context.Empleados
            .AsNoTracking()
            .Where(e => e.Estatus == EstatusEmpleado.Activo
                        && e.SueldoNetoSemanal != null
                        && e.SueldoNetoSemanal > 0)
            .ToListAsync();

        if (empleadosActivos.Count == 0)
            return (false, "No hay empleados activos con sueldo neto semanal capturado.", null);

        var asignacionesActivas = await _context.AsignacionesEmpleadoProyecto
            .AsNoTracking()
            .Where(a => a.Estado == EstadoAsignacionEmpleado.Activa)
            .ToDictionaryAsync(a => a.EmpleadoId, a => a.ProyectoId);

        var ajustes = (dto.Ajustes ?? [])
            .GroupBy(a => a.EmpleadoId)
            .ToDictionary(g => g.Key, g => g.First());

        var asistencias = (dto.Asistencias ?? [])
            .GroupBy(a => a.EmpleadoId)
            .ToDictionary(g => g.Key, g => g.First().Dias ?? []);

        var totalDiasPeriodo = (dto.FechaFin.Date - dto.FechaInicio.Date).Days + 1;

        var periodo = new PeriodoNomina
        {
            FechaInicio = dto.FechaInicio.Date,
            FechaFin = dto.FechaFin.Date,
            FechaGeneracion = DateTime.UtcNow,
            GeneradoPorId = usuarioId,
            Estado = EstadoPeriodoNomina.Generada
        };

        var excepcionesAsistencia = new List<AsistenciaDiaria>();

        foreach (var e in empleadosActivos)
        {
            ajustes.TryGetValue(e.Id, out var ajuste);
            if (ajuste?.Excluido == true) continue; // no se le paga en este periodo (p.ej. freelance)

            var sueldoDiario = e.SueldoNetoSemanal!.Value / 6m;
            asistencias.TryGetValue(e.Id, out var dias);
            dias ??= [];

            var faltas = dias.Count(d => d.Estado == "Falta");
            var retardos = dias.Count(d => d.Estado == "Retardo");
            var horasExtra = dias.Sum(d => d.HorasExtra);

            var diasPresentes = totalDiasPeriodo - faltas;
            var penalizacionPorRetardos = retardos / 3; // 3 retardos = 1 falta
            var diasEfectivos = Math.Max(0, diasPresentes - penalizacionPorRetardos);

            var bruto = diasEfectivos * sueldoDiario + horasExtra * TarifaHoraExtra;
            var descuento = e.CreditoInfonavit ? (e.CuotaInfonavit ?? 0m) : 0m;
            asignacionesActivas.TryGetValue(e.Id, out var proyectoId);

            var montoAjuste = ajuste?.MontoAjuste ?? 0m;

            periodo.Detalles.Add(new NominaDetalle
            {
                EmpleadoId = e.Id,
                ProyectoId = proyectoId == 0 ? null : proyectoId,
                SueldoDiario = sueldoDiario,
                DiasTrabajados = diasPresentes,
                Faltas = faltas,
                Retardos = retardos,
                HorasExtra = horasExtra,
                SueldoBruto = bruto,
                DescuentoInfonavit = descuento,
                MontoAjuste = montoAjuste,
                MotivoAjuste = ajuste?.MotivoAjuste,
                SueldoNeto = bruto - descuento + montoAjuste,
                Pagado = false
            });

            foreach (var d in dias.Where(d => d.Estado != "Presente" || d.HorasExtra != 0))
            {
                excepcionesAsistencia.Add(new AsistenciaDiaria
                {
                    EmpleadoId = e.Id,
                    Fecha = d.Fecha.Date,
                    Estado = Enum.Parse<EstadoAsistencia>(d.Estado),
                    HorasExtra = d.HorasExtra
                });
            }
        }

        if (periodo.Detalles.Count == 0)
            return (false, "Todos los empleados fueron excluidos de este periodo.", null);

        _context.PeriodosNomina.Add(periodo);
        if (excepcionesAsistencia.Count > 0)
            _context.AsistenciasDiarias.AddRange(excepcionesAsistencia);
        await _context.SaveChangesAsync();

        var (uid, uname) = GetUsuarioInfo();
        await _bitacora.RegistrarAsync(uid, uname, "Generó", "Nómina",
            $"Periodo de nómina {periodo.FechaInicio:dd/MM/yyyy} - {periodo.FechaFin:dd/MM/yyyy} generado con {periodo.Detalles.Count} empleado(s)");

        var creado = await _context.PeriodosNomina
            .AsNoTracking()
            .Include(p => p.Detalles).ThenInclude(d => d.Empleado)
            .Include(p => p.Detalles).ThenInclude(d => d.Proyecto)
            .FirstAsync(p => p.Id == periodo.Id);

        return (true, $"Periodo de nómina generado con {periodo.Detalles.Count} empleado(s).", ToDto(creado, incluirDetalles: true));
    }

    public async Task<(bool Success, string Message)> MarcarPeriodoPagadoAsync(int periodoId)
    {
        var periodo = await _context.PeriodosNomina
            .Include(p => p.Detalles)
            .FirstOrDefaultAsync(p => p.Id == periodoId);
        if (periodo is null) return (false, "Periodo de nómina no encontrado.");

        if (periodo.Estado == EstadoPeriodoNomina.Pagada)
            return (false, "Este periodo ya está marcado como pagado.");

        var ahora = DateTime.UtcNow;
        foreach (var d in periodo.Detalles.Where(d => !d.Pagado))
        {
            d.Pagado = true;
            d.FechaPago = ahora;
        }
        periodo.Estado = EstadoPeriodoNomina.Pagada;

        await _context.SaveChangesAsync();

        var (uid, uname) = GetUsuarioInfo();
        await _bitacora.RegistrarAsync(uid, uname, "Marcó pagado", "Nómina",
            $"Periodo de nómina {periodo.FechaInicio:dd/MM/yyyy} - {periodo.FechaFin:dd/MM/yyyy} marcado como pagado ({periodo.Detalles.Count} empleado(s))");

        var proyectosDelPeriodo = periodo.Detalles
            .Where(d => d.ProyectoId != null)
            .Select(d => d.ProyectoId!.Value)
            .Distinct();
        foreach (var proyectoId in proyectosDelPeriodo)
            await _gastoSemanalService.AsociarSiCompletoAsync(proyectoId, periodoId);

        return (true, "Periodo marcado como pagado.");
    }

    public async Task<(bool Success, string Message)> MarcarDetallePagadoAsync(int detalleId)
    {
        var detalle = await _context.NominaDetalles
            .Include(d => d.PeriodoNomina).ThenInclude(p => p!.Detalles)
            .Include(d => d.Empleado)
            .FirstOrDefaultAsync(d => d.Id == detalleId);
        if (detalle is null) return (false, "Registro de nómina no encontrado.");

        if (detalle.Pagado) return (false, "Este pago ya estaba registrado.");

        detalle.Pagado = true;
        detalle.FechaPago = DateTime.UtcNow;

        if (detalle.PeriodoNomina is not null && detalle.PeriodoNomina.Detalles.All(d => d.Pagado))
            detalle.PeriodoNomina.Estado = EstadoPeriodoNomina.Pagada;

        await _context.SaveChangesAsync();

        var (uid, uname) = GetUsuarioInfo();
        await _bitacora.RegistrarAsync(uid, uname, "Marcó pagado", "Nómina",
            $"Pago de nómina de '{detalle.Empleado?.NombreCompleto}' marcado como pagado");

        if (detalle.ProyectoId != null)
            await _gastoSemanalService.AsociarSiCompletoAsync(detalle.ProyectoId.Value, detalle.PeriodoNominaId);

        return (true, "Pago registrado correctamente.");
    }

    public async Task<List<HistorialNominaEmpleadoDto>> GetHistorialEmpleadoAsync(int empleadoId) =>
        await _context.NominaDetalles
            .AsNoTracking()
            .Include(d => d.PeriodoNomina)
            .Include(d => d.Proyecto)
            .Where(d => d.EmpleadoId == empleadoId)
            .OrderByDescending(d => d.PeriodoNomina!.FechaInicio)
            .Select(d => new HistorialNominaEmpleadoDto
            {
                PeriodoNominaId = d.PeriodoNominaId,
                FechaInicio = d.PeriodoNomina!.FechaInicio,
                FechaFin = d.PeriodoNomina.FechaFin,
                ProyectoNombre = d.Proyecto != null ? d.Proyecto.Nombre : null,
                SueldoBruto = d.SueldoBruto,
                DescuentoInfonavit = d.DescuentoInfonavit,
                MontoAjuste = d.MontoAjuste,
                MotivoAjuste = d.MotivoAjuste,
                SueldoNeto = d.SueldoNeto,
                Pagado = d.Pagado,
                FechaPago = d.FechaPago
            })
            .ToListAsync();

    public async Task<List<CostoProyectoNominaDto>> GetCostosPorProyectoAsync() =>
        await _context.NominaDetalles
            .AsNoTracking()
            .Where(d => d.ProyectoId != null)
            .GroupBy(d => new { d.ProyectoId, ProyectoNombre = d.Proyecto!.Nombre })
            .Select(g => new CostoProyectoNominaDto
            {
                ProyectoId = g.Key.ProyectoId!.Value,
                ProyectoNombre = g.Key.ProyectoNombre,
                TotalManoDeObra = g.Sum(d => d.SueldoNeto),
                TotalRegistros = g.Count()
            })
            .OrderByDescending(c => c.TotalManoDeObra)
            .ToListAsync();

    public async Task<List<PagoNominaDto>> GetPagosAsync() =>
        await _context.NominaDetalles
            .AsNoTracking()
            .Include(d => d.Empleado)
            .Include(d => d.Proyecto)
            .Include(d => d.PeriodoNomina)
            .Where(d => d.Pagado)
            .OrderByDescending(d => d.FechaPago)
            .Select(d => new PagoNominaDto
            {
                EmpleadoId = d.EmpleadoId,
                EmpleadoNombre = d.Empleado!.NombreCompleto,
                ProyectoId = d.ProyectoId,
                ProyectoNombre = d.Proyecto != null ? d.Proyecto.Nombre : null,
                SueldoNeto = d.SueldoNeto,
                FechaPago = d.FechaPago!.Value,
                FechaInicioPeriodo = d.PeriodoNomina!.FechaInicio,
                FechaFinPeriodo = d.PeriodoNomina.FechaFin
            })
            .ToListAsync();

    public async Task<byte[]> GenerarReportePdfAsync(int periodoId)
    {
        var periodo = await GetPeriodoByIdAsync(periodoId);
        if (periodo is null) return [];

        return Document.Create(container => new PeriodoNominaDocument(periodo).Compose(container))
            .GeneratePdf();
    }
}
