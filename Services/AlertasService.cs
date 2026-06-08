using BLL_ConstruccionAPI.DTOs.Alertas;
using BLL_ConstruccionAPI.Models.Enums;
using BLL_ConstruccionAPI.Repositories.Interfaces;
using BLL_ConstruccionAPI.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using BLL_ConstruccionAPI.Data;

namespace BLL_ConstruccionAPI.Services;

public class AlertasService : IAlertasService
{
    private readonly IMaterialesRepository _materialesRepo;
    private readonly IFasesRepository _fasesRepo;
    private readonly IProyectosRepository _proyectosRepo;
    private readonly IHerramientasRepository _herramientasRepo;
    private readonly AppDbContext _context;

    public AlertasService(
        IMaterialesRepository materialesRepo,
        IFasesRepository fasesRepo,
        IProyectosRepository proyectosRepo,
        IHerramientasRepository herramientasRepo,
        AppDbContext context)
    {
        _materialesRepo = materialesRepo;
        _fasesRepo = fasesRepo;
        _proyectosRepo = proyectosRepo;
        _herramientasRepo = herramientasRepo;
        _context = context;
    }

    public async Task<List<AlertaDto>> GetStockBajoAsync()
    {
        var stocks = await _context.AlmacenCentral
            .AsNoTracking()
            .Include(ac => ac.Material)
            .Where(ac => ac.Material != null && ac.Material.Activo && ac.Stock <= ac.Material.StockMinimo)
            .ToListAsync();

        var alertas = new List<AlertaDto>();
        foreach (var ac in stocks)
        {
            var nombre = ac.Material!.Nombre;
            string tipo, mensaje;

            if (ac.Stock == 0)
            {
                tipo = "StockCritico";
                mensaje = $"{nombre} no tiene stock disponible";
            }
            else
            {
                tipo = "StockBajo";
                mensaje = $"{nombre} tiene stock bajo ({ac.Stock} unidades)";
            }

            alertas.Add(new AlertaDto
            {
                Tipo = tipo,
                Severidad = ac.Stock == 0 ? "Alta" : "Media",
                Mensaje = mensaje,
                Referencia = "/materiales"
            });
        }

        return alertas;
    }

    public async Task<List<AlertaDto>> GetFasesAtrasadasAsync()
    {
        var fases = await _fasesRepo.GetAtrasadasAsync();
        var hoy = DateTime.UtcNow.Date;

        return fases.Select(f =>
        {
            var dias = (hoy - f.FechaLimite.Date).Days;
            return new AlertaDto
            {
                Tipo = "FaseAtrasada",
                Severidad = "Alta",
                Mensaje = $"Fase '{f.Nombre}' del proyecto '{f.Proyecto!.Nombre}' lleva {dias} día(s) de retraso",
                Referencia = $"/proyectos/{f.ProyectoId}"
            };
        }).ToList();
    }

    public async Task<List<AlertaDto>> GetFasesPorVencerAsync()
    {
        var fases = await _fasesRepo.GetPorVencerAsync();
        var hoy = DateTime.UtcNow.Date;

        return fases.Select(f =>
        {
            var dias = (f.FechaLimite.Date - hoy).Days;
            var mensaje = dias == 0
                ? $"Fase '{f.Nombre}' vence HOY"
                : $"Fase '{f.Nombre}' del proyecto '{f.Proyecto!.Nombre}' vence en {dias} día(s)";

            return new AlertaDto
            {
                Tipo = "FasePorVencer",
                Severidad = "Media",
                Mensaje = mensaje,
                Referencia = $"/proyectos/{f.ProyectoId}"
            };
        }).ToList();
    }

    public async Task<List<AlertaDto>> GetProyectosSinFasesAsync()
    {
        var proyectos = await _context.Proyectos
            .AsNoTracking()
            .Where(p => p.Activo && p.Estado == EstadoProyecto.Activo
                && !_context.FaseProyectos.Any(f => f.ProyectoId == p.Id))
            .ToListAsync();

        return proyectos.Select(p => new AlertaDto
        {
            Tipo = "ProyectoSinFases",
            Severidad = "Media",
            Mensaje = $"El proyecto '{p.Nombre}' no tiene fases registradas",
            Referencia = $"/proyectos/{p.Id}"
        }).ToList();
    }

    public async Task<List<AlertaDto>> GetHerramientasSinDevolverAsync()
    {
        var limite = DateTime.UtcNow.AddDays(-15);
        var asignaciones = await _context.AsignacionesHerramienta
            .AsNoTracking()
            .Include(a => a.Herramienta)
            .Include(a => a.Proyecto)
            .Where(a => a.Estado == EstadoAsignacion.Asignada && a.FechaAsignacion <= limite)
            .ToListAsync();

        return asignaciones.Select(a =>
        {
            var dias = (int)(DateTime.UtcNow - a.FechaAsignacion).TotalDays;
            return new AlertaDto
            {
                Tipo = "HerramientaSinDevolver",
                Severidad = "Media",
                Mensaje = $"'{a.Herramienta!.Nombre}' lleva {dias} días asignada al proyecto '{a.Proyecto!.Nombre}' sin devolver",
                Referencia = "/herramientas"
            };
        }).ToList();
    }

    public async Task<List<AlertaDto>> GetSinHerramientasDisponiblesAsync()
    {
        var herramientas = await _context.Herramientas
            .AsNoTracking()
            .Include(h => h.CategoriaHerramienta)
            .Where(h => h.Activo)
            .ToListAsync();

        var alertas = new List<AlertaDto>();

        var grupos = herramientas.GroupBy(h => h.CategoriaHerramientaId);
        foreach (var grupo in grupos)
        {
            var todas = grupo.ToList();
            var todasOcupadas = todas.All(h =>
                h.Estado == EstadoHerramienta.Asignada || h.Estado == EstadoHerramienta.Mantenimiento);

            if (todasOcupadas)
            {
                var nombreCategoria = todas.First().CategoriaHerramienta?.Nombre ?? $"Categoría {grupo.Key}";
                alertas.Add(new AlertaDto
                {
                    Tipo = "SinHerramientasDisponibles",
                    Severidad = "Alta",
                    Mensaje = $"No hay herramientas disponibles en la categoría '{nombreCategoria}'",
                    Referencia = "/herramientas"
                });
            }
        }

        return alertas;
    }

    public async Task<List<AlertaDto>> GetProyectosConFasesCompletadasAsync()
    {
        var proyectos = await _context.Proyectos
            .AsNoTracking()
            .Where(p => p.Activo && p.Estado == EstadoProyecto.Activo
                && _context.FaseProyectos.Any(f => f.ProyectoId == p.Id)
                && !_context.FaseProyectos.Any(f => f.ProyectoId == p.Id && f.Estado != EstadoFase.Completada))
            .ToListAsync();

        return proyectos.Select(p => new AlertaDto
        {
            Tipo = "ProyectoConFasesCompletadas",
            Severidad = "Media",
            Mensaje = p.Nombre,
            Referencia = $"/proyectos/{p.Id}"
        }).ToList();
    }

    public async Task<ResumenAlertasDto> GetResumenAsync()
    {
        var stock             = await GetStockBajoAsync();
        var fasesAtrasadas    = await GetFasesAtrasadasAsync();
        var fasesPorVencer    = await GetFasesPorVencerAsync();
        var proyectosSinFases = await GetProyectosSinFasesAsync();
        var herramientas      = await GetHerramientasSinDevolverAsync();
        var sinDisponibles    = await GetSinHerramientasDisponiblesAsync();

        var detalle = stock
            .Concat(fasesAtrasadas)
            .Concat(fasesPorVencer)
            .Concat(proyectosSinFases)
            .Concat(herramientas)
            .Concat(sinDisponibles)
            .OrderByDescending(a => a.Severidad == "Alta")
            .ThenBy(a => a.Fecha)
            .ToList();

        return new ResumenAlertasDto
        {
            TotalAlertas               = detalle.Count,
            StockBajo                  = stock.Count,
            FasesAtrasadas             = fasesAtrasadas.Count,
            FasesPorVencer             = fasesPorVencer.Count,
            ProyectosSinFases          = proyectosSinFases.Count,
            HerramientasSinDevolver    = herramientas.Count,
            SinHerramientasDisponibles = sinDisponibles.Count,
            Detalle                    = detalle
        };
    }
}
