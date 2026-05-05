using System.Collections.Concurrent;

namespace BLL_ConstruccionAPI.Helpers;

public class RateLimitHelper
{
    private readonly ConcurrentDictionary<string, List<DateTime>> _requestLog = new();
    private readonly TimeSpan _ventanaAnalisis = TimeSpan.FromMinutes(15);
    private readonly TimeSpan _ventanaEntreSolicitudes = TimeSpan.FromSeconds(60);

    /// <summary>
    /// Verifica si se puede enviar un código (respeta límites de rate limiting)
    /// </summary>
    /// <param name="identificador">Email o IP del solicitante</param>
    /// <param name="maxSolicitudesPorVentana">Máximo de solicitudes en 15 minutos (default: 5)</param>
    /// <returns>(Permitido, Mensaje de error si no se permite)</returns>
    public (bool Permitido, string? MensajeError) PuedeEnviarCodigo(
        string identificador, 
        int maxSolicitudesPorVentana = 5)
    {
        var ahora = DateTime.UtcNow;

        // Obtener o crear registro de solicitudes para este identificador
        var solicitudes = _requestLog.GetOrAdd(identificador, _ => new List<DateTime>());

        lock (solicitudes)
        {
            // Limpiar solicitudes antiguas (fuera de ventana de análisis)
            solicitudes.RemoveAll(s => ahora - s > _ventanaAnalisis);

            // Verificar última solicitud (mínimo 60s entre solicitudes)
            if (solicitudes.Any())
            {
                var ultimaSolicitud = solicitudes.Max();
                if (ahora - ultimaSolicitud < _ventanaEntreSolicitudes)
                {
                    var segundosRestantes = (int)(_ventanaEntreSolicitudes - (ahora - ultimaSolicitud)).TotalSeconds;
                    return (false, $"Debes esperar {segundosRestantes} segundos antes de solicitar otro código.");
                }
            }

            // Verificar número de solicitudes en ventana
            if (solicitudes.Count >= maxSolicitudesPorVentana)
            {
                return (false, "Has excedido el límite de solicitudes. Intenta más tarde.");
            }

            // Registrar nueva solicitud
            solicitudes.Add(ahora);
            return (true, null);
        }
    }

    /// <summary>
    /// Limpia registros antiguos (llamar periódicamente para liberar memoria)
    /// </summary>
    public void LimpiarRegistrosAntiguos()
    {
        var ahora = DateTime.UtcNow;
        var clavesAEliminar = new List<string>();

        foreach (var kvp in _requestLog)
        {
            lock (kvp.Value)
            {
                kvp.Value.RemoveAll(s => ahora - s > _ventanaAnalisis);
                if (!kvp.Value.Any())
                {
                    clavesAEliminar.Add(kvp.Key);
                }
            }
        }

        foreach (var clave in clavesAEliminar)
        {
            _requestLog.TryRemove(clave, out _);
        }
    }
}
