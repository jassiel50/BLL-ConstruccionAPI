using BLL_ConstruccionAPI.Models.Enums;
using BLL_ConstruccionAPI.Models.Inventario.Herramientas;
using BLL_ConstruccionAPI.Models.Inventario.Materiales;
using BLL_ConstruccionAPI.Models.Inventario.Proyectos;

namespace BLL_ConstruccionAPI.Models.Inventario.Perdidas;

public class RegistroPerdida
{
    public int Id { get; set; }
    public TipoPerdida Tipo { get; set; }           // Material o Herramienta
    public int? MaterialId { get; set; }            // Si es material
    public int? HerramientaId { get; set; }         // Si es herramienta
    public int? ProyectoId { get; set; }            // Dónde ocurrió (opcional)
    public int UsuarioReportaId { get; set; }       // Quien reporta
    public MotivoPerdida Motivo { get; set; }
    public decimal CantidadPerdida { get; set; }    // Solo aplica para materiales
    public string Descripcion { get; set; } = string.Empty;
    public DateTime FechaPerdida { get; set; }
    public DateTime FechaRegistro { get; set; } = DateTime.UtcNow;

    // Navegación
    public Material? Material { get; set; }
    public Herramienta? Herramienta { get; set; }
    public Proyecto? Proyecto { get; set; }
}
