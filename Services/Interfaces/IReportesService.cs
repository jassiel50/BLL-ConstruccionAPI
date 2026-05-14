namespace BLL_ConstruccionAPI.Services.Interfaces;

public interface IReportesService
{
    Task<byte[]> GenerarInventarioAsync();
    Task<byte[]> GenerarMovimientosAsync(DateTime desde, DateTime hasta);
    Task<byte[]> GenerarHerramientasAsync();
    Task<byte[]> GenerarProyectosAsync();
    Task<byte[]> GenerarPerdidasAsync(DateTime desde, DateTime hasta);
    Task<byte[]> GenerarPagosPorProyectoAsync(int proyectoId);
}
