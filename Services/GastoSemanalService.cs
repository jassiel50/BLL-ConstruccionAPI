using System.Security.Claims;
using BLL_ConstruccionAPI.Data;
using BLL_ConstruccionAPI.DTOs.GastosSemanales;
using BLL_ConstruccionAPI.Models.Inventario.Proyectos;
using BLL_ConstruccionAPI.Models.Nomina;
using BLL_ConstruccionAPI.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace BLL_ConstruccionAPI.Services;

public class GastoSemanalService : IGastoSemanalService
{
    private readonly AppDbContext _context;
    private readonly IBitacoraService _bitacora;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public GastoSemanalService(AppDbContext context, IBitacoraService bitacora, IHttpContextAccessor httpContextAccessor)
    {
        _context = context;
        _bitacora = bitacora;
        _httpContextAccessor = httpContextAccessor;
    }

    private (int Id, string Nombre, string Ip) GetUsuarioInfo()
    {
        var user = _httpContextAccessor.HttpContext?.User;
        var id = int.TryParse(user?.FindFirstValue(ClaimTypes.NameIdentifier), out var parsed) ? parsed : 0;
        var nombre = user?.FindFirstValue("nombreUsuario") ?? "Sistema";
        var ip = _httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString() ?? "";
        return (id, nombre, ip);
    }

    private static GastoSemanalDto MapToDto(GastoSemanal g, int diasDesdeUltimo = 0) => new()
    {
        Id = g.Id,
        ProyectoId = g.ProyectoId,
        Concepto = g.Concepto,
        Monto = g.Monto,
        FechaInicio = g.FechaInicio,
        FechaFin = g.FechaFin,
        Tipo = g.Tipo,
        Observaciones = g.Observaciones,
        NumPersonas = g.NumPersonas,
        FechaRegistro = g.FechaRegistro,
        DiasDesdeUltimo = diasDesdeUltimo,
        PeriodoNominaId = g.PeriodoNominaId
    };

    public async Task<List<GastoSemanalDto>> GetByProyectoAsync(int proyectoId)
    {
        var lista = await _context.GastosSemanales
            .Where(g => g.ProyectoId == proyectoId)
            .OrderByDescending(g => g.FechaRegistro)
            .ToListAsync();

        var hoy = DateTime.UtcNow.Date;
        return lista.Select(g => MapToDto(g, (int)(hoy - g.FechaFin.Date).TotalDays)).ToList();
    }

    public async Task<(bool Success, GastoSemanalDto? Data)> CreateAsync(int proyectoId, GastoSemanalRequestDto dto)
    {
        var existe = await _context.Proyectos.AnyAsync(p => p.Id == proyectoId);
        if (!existe) return (false, null);

        var entity = new GastoSemanal
        {
            ProyectoId = proyectoId,
            Concepto = dto.Concepto,
            Monto = dto.Monto,
            FechaInicio = dto.FechaInicio,
            FechaFin = dto.FechaFin,
            Tipo = dto.Tipo,
            Observaciones = dto.Observaciones,
            NumPersonas = dto.NumPersonas,
            FechaRegistro = DateTime.UtcNow
        };

        _context.GastosSemanales.Add(entity);
        await _context.SaveChangesAsync();

        var (uid, uname, ip) = GetUsuarioInfo();
        await _bitacora.RegistrarAsync(uid, uname, "Registró gasto semanal", "GastoSemanal",
            $"Gasto semanal '{entity.Concepto}' (${entity.Monto:N2}) registrado en proyecto ID {proyectoId}.", ip);

        var dias = (int)(DateTime.UtcNow.Date - entity.FechaFin.Date).TotalDays;
        return (true, MapToDto(entity, dias));
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var entity = await _context.GastosSemanales.FindAsync(id);
        if (entity is null) return false;

        _context.GastosSemanales.Remove(entity);
        await _context.SaveChangesAsync();

        var (uid, uname, ip) = GetUsuarioInfo();
        await _bitacora.RegistrarAsync(uid, uname, "Eliminó gasto semanal", "GastoSemanal",
            $"Gasto semanal ID {entity.Id} eliminado del proyecto ID {entity.ProyectoId}.", ip);

        return true;
    }

    public async Task<(bool Found, GastoSemanalDto? Data)> GetUltimoAsync(int proyectoId)
    {
        var ultimo = await _context.GastosSemanales
            .Where(g => g.ProyectoId == proyectoId)
            .OrderByDescending(g => g.FechaFin)
            .FirstOrDefaultAsync();

        if (ultimo is null) return (false, null);

        var dias = (int)(DateTime.UtcNow.Date - ultimo.FechaFin.Date).TotalDays;
        return (true, MapToDto(ultimo, dias));
    }

    public async Task<List<NominaDisponibleProyectoDto>> GetNominaDisponibleParaProyectoAsync(int proyectoId)
    {
        var periodosUsados = await _context.GastosSemanales
            .Where(g => g.ProyectoId == proyectoId && g.PeriodoNominaId != null)
            .Select(g => g.PeriodoNominaId!.Value)
            .ToListAsync();

        var detalles = await _context.NominaDetalles
            .AsNoTracking()
            .Include(d => d.Empleado)
            .Include(d => d.PeriodoNomina)
            .Where(d => d.ProyectoId == proyectoId && !periodosUsados.Contains(d.PeriodoNominaId))
            .ToListAsync();

        return detalles
            .GroupBy(d => d.PeriodoNominaId)
            .Select(g => new NominaDisponibleProyectoDto
            {
                PeriodoNominaId = g.Key,
                FechaInicio = g.First().PeriodoNomina!.FechaInicio,
                FechaFin = g.First().PeriodoNomina!.FechaFin,
                MontoTotal = g.Sum(d => d.SueldoNeto),
                NumEmpleados = g.Count(),
                Empleados = g.Select(d => new EmpleadoMontoDto
                {
                    EmpleadoNombre = d.Empleado?.NombreCompleto ?? string.Empty,
                    SueldoNeto = d.SueldoNeto
                }).ToList()
            })
            .OrderByDescending(x => x.FechaInicio)
            .ToList();
    }

    public async Task<(bool Success, string Message, GastoSemanalDto? Data)> CrearDesdeNominaAsync(int proyectoId, int periodoNominaId)
    {
        var yaUsado = await _context.GastosSemanales
            .AnyAsync(g => g.ProyectoId == proyectoId && g.PeriodoNominaId == periodoNominaId);
        if (yaUsado)
            return (false, "Esta nómina ya fue asociada a un gasto de este proyecto.", null);

        var detalles = await _context.NominaDetalles
            .AsNoTracking()
            .Include(d => d.PeriodoNomina)
            .Where(d => d.ProyectoId == proyectoId && d.PeriodoNominaId == periodoNominaId)
            .ToListAsync();

        if (detalles.Count == 0)
            return (false, "No hay empleados de este proyecto en la nómina seleccionada.", null);

        var dto = await CrearGastoDesdeNominaAsync(proyectoId, periodoNominaId, detalles, automatico: false);
        return (true, "Gasto registrado a partir de la nómina.", dto);
    }

    public async Task AsociarSiCompletoAsync(int proyectoId, int periodoNominaId)
    {
        var yaUsado = await _context.GastosSemanales
            .AnyAsync(g => g.ProyectoId == proyectoId && g.PeriodoNominaId == periodoNominaId);
        if (yaUsado) return;

        var detalles = await _context.NominaDetalles
            .AsNoTracking()
            .Include(d => d.PeriodoNomina)
            .Where(d => d.ProyectoId == proyectoId && d.PeriodoNominaId == periodoNominaId)
            .ToListAsync();

        if (detalles.Count == 0 || detalles.Any(d => !d.Pagado)) return;

        await CrearGastoDesdeNominaAsync(proyectoId, periodoNominaId, detalles, automatico: true);
    }

    private async Task<GastoSemanalDto> CrearGastoDesdeNominaAsync(
        int proyectoId, int periodoNominaId, List<NominaDetalle> detalles, bool automatico)
    {
        var periodo = detalles[0].PeriodoNomina!;
        var entity = new GastoSemanal
        {
            ProyectoId = proyectoId,
            Concepto = $"Nómina {periodo.FechaInicio:dd/MM/yyyy} - {periodo.FechaFin:dd/MM/yyyy}",
            Monto = detalles.Sum(d => d.SueldoNeto),
            FechaInicio = periodo.FechaInicio,
            FechaFin = periodo.FechaFin,
            Tipo = "Nomina",
            NumPersonas = detalles.Count,
            PeriodoNominaId = periodoNominaId,
            FechaRegistro = DateTime.UtcNow
        };

        _context.GastosSemanales.Add(entity);
        await _context.SaveChangesAsync();

        var (uid, uname, ip) = GetUsuarioInfo();
        var descripcion = automatico
            ? $"Gasto semanal '{entity.Concepto}' (${entity.Monto:N2}) generado automáticamente al completarse el pago de la nómina #{periodoNominaId} en proyecto ID {proyectoId}."
            : $"Gasto semanal '{entity.Concepto}' (${entity.Monto:N2}) generado a partir de la nómina #{periodoNominaId} en proyecto ID {proyectoId}.";
        await _bitacora.RegistrarAsync(uid, uname, "Registró gasto semanal desde nómina", "GastoSemanal", descripcion, ip);

        var dias = (int)(DateTime.UtcNow.Date - entity.FechaFin.Date).TotalDays;
        return MapToDto(entity, dias);
    }
}
