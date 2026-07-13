using BLL_ConstruccionAPI.Data;
using BLL_ConstruccionAPI.DTOs.Reportes;
using BLL_ConstruccionAPI.Helpers;
using BLL_ConstruccionAPI.Models.Auth;
using BLL_ConstruccionAPI.Models.Reportes;
using BLL_ConstruccionAPI.Repositories.Interfaces;
using BLL_ConstruccionAPI.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace BLL_ConstruccionAPI.Services;

public class ConfiguracionReporteService : IConfiguracionReporteService
{
    private static readonly HashSet<string> SeccionesValidas =
        ["inventario", "proyectos", "movimientos", "herramientas", "perdidas", "avance_cliente"];

    private const string SeccionAvanceCliente = "avance_cliente";

    private readonly IConfiguracionReporteRepository _repo;
    private readonly IReportesService _reportesService;
    private readonly IEmailService _emailService;
    private readonly IUsuarioRepository _usuarioRepo;
    private readonly AppDbContext _context;

    public ConfiguracionReporteService(
        IConfiguracionReporteRepository repo,
        IReportesService reportesService,
        IEmailService emailService,
        IUsuarioRepository usuarioRepo,
        AppDbContext context)
    {
        _repo = repo;
        _reportesService = reportesService;
        _emailService = emailService;
        _usuarioRepo = usuarioRepo;
        _context = context;
    }

    public async Task<IEnumerable<ConfiguracionReporteResponseDto>> GetMisConfiguracionesAsync(int usuarioId)
    {
        var configs = await _repo.GetByUsuarioIdAsync(usuarioId);
        return configs.Select(ConfiguracionReporteResponseDto.FromEntity);
    }

    public async Task<ConfiguracionReporteResponseDto?> GetByIdAsync(int id, int usuarioId)
    {
        var config = await _repo.GetByIdAsync(id);
        if (config is null || config.UsuarioId != usuarioId) return null;
        return ConfiguracionReporteResponseDto.FromEntity(config);
    }

    public async Task<(bool Success, string Message, ConfiguracionReporteResponseDto? Data)> CreateAsync(
        ConfiguracionReporteRequestDto dto, int usuarioId)
    {
        var validacion = ValidarSecciones(dto.Secciones);
        if (validacion is not null) return (false, validacion, null);

        var (destOk, destMsg, destinatarios) = ValidarDestinatarios(dto.Destinatarios);
        if (!destOk) return (false, destMsg!, null);

        var proyectoMsg = await ValidarProyectoAsync(dto.Secciones, dto.ProyectoId);
        if (proyectoMsg is not null) return (false, proyectoMsg, null);

        var config = new ConfiguracionReporte
        {
            UsuarioId = usuarioId,
            Nombre = dto.Nombre.Trim(),
            Frecuencia = dto.Frecuencia,
            HoraEnvio = dto.HoraEnvio,
            Secciones = JsonSerializer.Serialize(dto.Secciones),
            Destinatarios = destinatarios.Count > 0 ? JsonSerializer.Serialize(destinatarios) : null,
            ProyectoId = dto.Secciones.Contains(SeccionAvanceCliente) ? dto.ProyectoId : null,
            Activo = dto.Activo,
            FechaCreacion = DateTime.UtcNow
        };

        await _repo.CreateAsync(config);
        return (true, "Configuración de reporte creada correctamente.", ConfiguracionReporteResponseDto.FromEntity(config));
    }

    public async Task<(bool Success, string Message)> UpdateAsync(int id, ConfiguracionReporteRequestDto dto, int usuarioId)
    {
        var config = await _repo.GetByIdAsync(id);
        if (config is null || config.UsuarioId != usuarioId)
            return (false, "Configuración no encontrada.");

        var validacion = ValidarSecciones(dto.Secciones);
        if (validacion is not null) return (false, validacion);

        var (destOk, destMsg, destinatarios) = ValidarDestinatarios(dto.Destinatarios);
        if (!destOk) return (false, destMsg!);

        var proyectoMsg = await ValidarProyectoAsync(dto.Secciones, dto.ProyectoId);
        if (proyectoMsg is not null) return (false, proyectoMsg);

        config.Nombre = dto.Nombre.Trim();
        config.Frecuencia = dto.Frecuencia;
        config.HoraEnvio = dto.HoraEnvio;
        config.Secciones = JsonSerializer.Serialize(dto.Secciones);
        config.Destinatarios = destinatarios.Count > 0 ? JsonSerializer.Serialize(destinatarios) : null;
        config.ProyectoId = dto.Secciones.Contains(SeccionAvanceCliente) ? dto.ProyectoId : null;
        config.Activo = dto.Activo;

        await _repo.UpdateAsync(config);
        return (true, "Configuración actualizada correctamente.");
    }

    public async Task<(bool Success, string Message)> DeleteAsync(int id, int usuarioId)
    {
        var config = await _repo.GetByIdAsync(id);
        if (config is null || config.UsuarioId != usuarioId)
            return (false, "Configuración no encontrada.");

        await _repo.DeleteAsync(config);
        return (true, "Configuración eliminada correctamente.");
    }

    public async Task<(bool Success, string Message)> EnviarAhoraAsync(int id, int usuarioId)
    {
        var config = await _repo.GetByIdAsync(id);
        if (config is null || config.UsuarioId != usuarioId)
            return (false, "Configuración no encontrada.");

        var usuario = await _usuarioRepo.GetByIdAsync(usuarioId);
        if (usuario is null) return (false, "Usuario no encontrado.");

        var destinatarios = ResolverDestinatarios(config, usuario);
        await EnviarReportesDeConfiguracionAsync(config, destinatarios, usuario.Nombre);

        config.UltimoEnvio = DateTime.UtcNow;
        await _repo.UpdateAsync(config);

        return (true, "Reporte enviado correctamente.");
    }

    /// <summary>Devuelve la lista de correos a los que debe enviarse una configuración, con respaldo a los correos del dueño (principal y secundario).</summary>
    public static List<string> ResolverDestinatarios(ConfiguracionReporte config, Usuario duenio)
    {
        if (string.IsNullOrWhiteSpace(config.Destinatarios)) return duenio.CorreosNotificacion().ToList();

        var lista = JsonSerializer.Deserialize<List<string>>(config.Destinatarios) ?? [];
        return lista.Count > 0 ? lista : duenio.CorreosNotificacion().ToList();
    }

    internal async Task EnviarReportesDeConfiguracionAsync(ConfiguracionReporte config, List<string> destinatarios, string nombreDueno)
    {
        var secciones = JsonSerializer.Deserialize<List<string>>(config.Secciones) ?? [];
        var hoy = DateTime.UtcNow;
        var hace30 = hoy.AddDays(-30);

        foreach (var seccion in secciones)
        {
            try
            {
                byte[] pdf;
                string tipo;
                string nombreArchivo;

                switch (seccion.ToLowerInvariant())
                {
                    case "inventario":
                        pdf = await _reportesService.GenerarInventarioAsync();
                        tipo = "Inventario";
                        nombreArchivo = $"Inventario_{hoy:yyyyMMdd}.pdf";
                        break;
                    case "proyectos":
                        pdf = await _reportesService.GenerarProyectosAsync();
                        tipo = "Proyectos";
                        nombreArchivo = $"Proyectos_{hoy:yyyyMMdd}.pdf";
                        break;
                    case "movimientos":
                        pdf = await _reportesService.GenerarMovimientosAsync(hace30, hoy);
                        tipo = "Movimientos (últimos 30 días)";
                        nombreArchivo = $"Movimientos_{hoy:yyyyMMdd}.pdf";
                        break;
                    case "herramientas":
                        pdf = await _reportesService.GenerarHerramientasAsync();
                        tipo = "Herramientas";
                        nombreArchivo = $"Herramientas_{hoy:yyyyMMdd}.pdf";
                        break;
                    case "perdidas":
                        pdf = await _reportesService.GenerarPerdidasAsync(hace30, hoy);
                        tipo = "Pérdidas (últimos 30 días)";
                        nombreArchivo = $"Perdidas_{hoy:yyyyMMdd}.pdf";
                        break;
                    case SeccionAvanceCliente:
                        if (config.ProyectoId is null) continue;
                        pdf = await _reportesService.GenerarAvanceClienteAsync(config.ProyectoId.Value);
                        if (pdf.Length == 0) continue;
                        tipo = "Avance de Proyecto";
                        nombreArchivo = $"Avance_Proyecto{config.ProyectoId}_{hoy:yyyyMMdd}.pdf";
                        break;
                    default:
                        continue;
                }

                foreach (var destinatario in destinatarios)
                    await _emailService.SendReporteProgramadoAsync(destinatario, nombreDueno, tipo, pdf, nombreArchivo);
            }
            catch
            {
                // una sección fallida no interrumpe las demás
            }
        }
    }

    private static string? ValidarSecciones(List<string> secciones)
    {
        if (secciones.Count == 0)
            return "Debe incluir al menos una sección.";

        var invalidas = secciones
            .Where(s => !SeccionesValidas.Contains(s.ToLowerInvariant()))
            .ToList();

        if (invalidas.Count > 0)
            return $"Secciones no válidas: {string.Join(", ", invalidas)}. Valores permitidos: {string.Join(", ", SeccionesValidas)}.";

        return null;
    }

    private async Task<string?> ValidarProyectoAsync(List<string> secciones, int? proyectoId)
    {
        if (!secciones.Contains(SeccionAvanceCliente)) return null;

        if (proyectoId is null)
            return "Selecciona el proyecto para el reporte de avance de cliente.";

        var existe = await _context.Proyectos.AnyAsync(p => p.Id == proyectoId.Value && p.Activo);
        if (!existe) return "El proyecto seleccionado no existe.";

        return null;
    }

    private const int MaxDestinatarios = 15;

    private static (bool Ok, string? Message, List<string> Limpios) ValidarDestinatarios(List<string> destinatarios)
    {
        var limpios = destinatarios
            .Where(d => !string.IsNullOrWhiteSpace(d))
            .Select(d => d.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        if (limpios.Count > MaxDestinatarios)
            return (false, $"No puedes agregar más de {MaxDestinatarios} destinatarios.", []);

        foreach (var correo in limpios)
        {
            if (!EsEmailValido(correo))
                return (false, $"'{correo}' no es una dirección de correo válida.", []);
        }

        return (true, null, limpios);
    }

    private static bool EsEmailValido(string email)
    {
        try { return new System.Net.Mail.MailAddress(email).Address.Equals(email, StringComparison.OrdinalIgnoreCase); }
        catch { return false; }
    }
}
