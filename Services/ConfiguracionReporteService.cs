using BLL_ConstruccionAPI.DTOs.Reportes;
using BLL_ConstruccionAPI.Models.Reportes;
using BLL_ConstruccionAPI.Repositories.Interfaces;
using BLL_ConstruccionAPI.Services.Interfaces;
using System.Text.Json;

namespace BLL_ConstruccionAPI.Services;

public class ConfiguracionReporteService : IConfiguracionReporteService
{
    private static readonly HashSet<string> SeccionesValidas =
        ["inventario", "proyectos", "movimientos", "herramientas", "perdidas"];

    private readonly IConfiguracionReporteRepository _repo;
    private readonly IReportesService _reportesService;
    private readonly IEmailService _emailService;
    private readonly IUsuarioRepository _usuarioRepo;

    public ConfiguracionReporteService(
        IConfiguracionReporteRepository repo,
        IReportesService reportesService,
        IEmailService emailService,
        IUsuarioRepository usuarioRepo)
    {
        _repo = repo;
        _reportesService = reportesService;
        _emailService = emailService;
        _usuarioRepo = usuarioRepo;
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

        var config = new ConfiguracionReporte
        {
            UsuarioId = usuarioId,
            Nombre = dto.Nombre.Trim(),
            Frecuencia = dto.Frecuencia,
            HoraEnvio = dto.HoraEnvio,
            Secciones = JsonSerializer.Serialize(dto.Secciones),
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

        config.Nombre = dto.Nombre.Trim();
        config.Frecuencia = dto.Frecuencia;
        config.HoraEnvio = dto.HoraEnvio;
        config.Secciones = JsonSerializer.Serialize(dto.Secciones);
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

        await EnviarReportesDeConfiguracionAsync(config, usuario.Email, usuario.Nombre);

        config.UltimoEnvio = DateTime.UtcNow;
        await _repo.UpdateAsync(config);

        return (true, "Reporte enviado correctamente.");
    }

    internal async Task EnviarReportesDeConfiguracionAsync(ConfiguracionReporte config, string email, string nombre)
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
                    default:
                        continue;
                }

                await _emailService.SendReporteProgramadoAsync(email, nombre, tipo, pdf, nombreArchivo);
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
}
