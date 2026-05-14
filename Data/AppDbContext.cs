using BLL_ConstruccionAPI.Models.Auth;
using BLL_ConstruccionAPI.Models.Enums;
using BLL_ConstruccionAPI.Models.Inventario;
using BLL_ConstruccionAPI.Models.Inventario.Cátalogos;
using BLL_ConstruccionAPI.Models.Inventario.Herramientas;
using BLL_ConstruccionAPI.Models.Inventario.Materiales;
using BLL_ConstruccionAPI.Models.Inventario.Perdidas;
using BLL_ConstruccionAPI.Models.Inventario.Proyectos;
using Microsoft.EntityFrameworkCore;

namespace BLL_ConstruccionAPI.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    // ─── AUTH ─────────────────────────────────────────────────────────────────
    public DbSet<Usuario> Usuarios { get; set; }
    public DbSet<Rol> Roles { get; set; }
    public DbSet<TokenSesion> TokensSesion { get; set; }
    public DbSet<Usuario2FA> Usuarios2FA { get; set; }
    public DbSet<LogAcceso> LogAccesos { get; set; }
    public DbSet<UsuarioMfaConfig> UsuariosMfaConfig { get; set; }
    public DbSet<MfaEmailCode> MfaEmailCodes { get; set; }
    public DbSet<PasswordResetCode> PasswordResetCodes { get; set; }

    // ─── CATÁLOGOS ────────────────────────────────────────────────────────────
    public DbSet<CategoriaMaterial> Categorias { get; set; }
    public DbSet<UnidadMedida> UnidadesMedida { get; set; }
    public DbSet<CategoriaHerramienta> CategoriasHerramienta { get; set; }

    // ─── PROVEEDORES Y CLIENTES ───────────────────────────────────────────────
    public DbSet<Proveedor> Proveedores { get; set; }
    public DbSet<Cliente> Clientes { get; set; }

    // ─── PROYECTOS ────────────────────────────────────────────────────────────
    public DbSet<Proyecto> Proyectos { get; set; }
    public DbSet<FaseProyecto> FaseProyectos { get; set; }
    public DbSet<GastoExtra> GastosExtras { get; set; }
    public DbSet<ChecklistItem> ChecklistItems { get; set; }
    public DbSet<ArchivoProyecto> ArchivosProyecto { get; set; }
    public DbSet<PagoCliente> PagosCliente { get; set; }

    // ─── MATERIALES ───────────────────────────────────────────────────────────
    public DbSet<Material> Materiales { get; set; }
    public DbSet<AlmacenCentral> AlmacenCentral { get; set; }
    public DbSet<AlmacenProyecto> AlmacenProyecto { get; set; }
    public DbSet<Entrada> Entradas { get; set; }
    public DbSet<EntradaDetalle> EntradasDetalle { get; set; }
    public DbSet<Salida> Salidas { get; set; }
    public DbSet<SalidaDetalle> SalidasDetalle { get; set; }

    // ─── DEVOLUCIONES MATERIAL ────────────────────────────────────────────────
    public DbSet<DevolucionMaterial> DevolucionesMaterial { get; set; }

    // ─── HERRAMIENTAS ─────────────────────────────────────────────────────────
    public DbSet<Herramienta> Herramientas { get; set; }
    public DbSet<AsignacionHerramienta> AsignacionesHerramienta { get; set; }

    // ─── PÉRDIDAS ─────────────────────────────────────────────────────────────
    public DbSet<RegistroPerdida> RegistrosPerdidas { get; set; }

    // ─── BITÁCORA ─────────────────────────────────────────────────────────────
    public DbSet<BitacoraActividad> Bitacora { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Evitar ciclos de relaciones en Proyecto
        modelBuilder.Entity<Proyecto>()
            .HasOne(p => p.Cliente)
            .WithMany()
            .HasForeignKey(p => p.ClienteId)
            .OnDelete(DeleteBehavior.Restrict);

        // Evitar ciclos en Entrada
        modelBuilder.Entity<Entrada>()
            .HasOne(e => e.Proveedor)
            .WithMany()
            .HasForeignKey(e => e.ProveedorId)
            .OnDelete(DeleteBehavior.Restrict);

        // Evitar ciclos en Salida
        modelBuilder.Entity<Salida>()
            .HasOne(s => s.Proyecto)
            .WithMany()
            .HasForeignKey(s => s.ProyectoId)
            .OnDelete(DeleteBehavior.Restrict);

        // Evitar ciclos en AsignacionHerramienta
        modelBuilder.Entity<AsignacionHerramienta>()
            .HasOne(a => a.Herramienta)
            .WithMany()
            .HasForeignKey(a => a.HerramientaId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<AsignacionHerramienta>()
            .HasOne(a => a.Proyecto)
            .WithMany()
            .HasForeignKey(a => a.ProyectoId)
            .OnDelete(DeleteBehavior.Restrict);

        // Decimales
        modelBuilder.Entity<Material>()
            .Property(m => m.StockMinimo)
            .HasColumnType("decimal(18,2)");

        modelBuilder.Entity<Material>()
            .Property(m => m.PrecioUnitario)
            .HasColumnType("decimal(18,2)");

        modelBuilder.Entity<AlmacenCentral>()
            .Property(a => a.Stock)
            .HasColumnType("decimal(18,2)");

        modelBuilder.Entity<AlmacenProyecto>()
            .Property(a => a.Stock)
            .HasColumnType("decimal(18,2)");

        modelBuilder.Entity<Entrada>()
            .Property(e => e.Total)
            .HasColumnType("decimal(18,2)");

        modelBuilder.Entity<EntradaDetalle>()
            .Property(e => e.Cantidad)
            .HasColumnType("decimal(18,2)");

        modelBuilder.Entity<EntradaDetalle>()
            .Property(e => e.PrecioUnitario)
            .HasColumnType("decimal(18,2)");

        modelBuilder.Entity<EntradaDetalle>()
            .Property(e => e.Subtotal)
            .HasColumnType("decimal(18,2)");

        modelBuilder.Entity<SalidaDetalle>()
            .Property(s => s.Cantidad)
            .HasColumnType("decimal(18,2)");

        modelBuilder.Entity<SalidaDetalle>()
            .Property(s => s.PrecioUnitario)
            .HasColumnType("decimal(18,2)");

        modelBuilder.Entity<Proyecto>()
            .Property(p => p.MontoContrato)
            .HasColumnType("decimal(18,2)");

        modelBuilder.Entity<Proyecto>()
            .Property(p => p.PresupuestoEstimado)
            .HasColumnType("decimal(18,2)");

        modelBuilder.Entity<GastoExtra>()
            .Property(g => g.Monto)
            .HasColumnType("decimal(18,2)");

        modelBuilder.Entity<GastoExtra>()
            .Property(g => g.MontoProveedor)
            .HasColumnType("decimal(18,2)");

        modelBuilder.Entity<GastoExtra>()
            .Property(g => g.CobradoCliente)
            .HasColumnType("decimal(18,2)");

        modelBuilder.Entity<GastoExtra>()
            .HasOne(g => g.Fase)
            .WithMany()
            .HasForeignKey(g => g.FaseId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<GastoExtra>()
            .HasOne(g => g.Proveedor)
            .WithMany()
            .HasForeignKey(g => g.ProveedorId)
            .OnDelete(DeleteBehavior.SetNull);

        // ─── ChecklistItem ────────────────────────────────────────────────────
        modelBuilder.Entity<ChecklistItem>()
            .HasOne(c => c.Proyecto)
            .WithMany()
            .HasForeignKey(c => c.ProyectoId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<ChecklistItem>()
            .HasOne(c => c.Fase)
            .WithMany()
            .HasForeignKey(c => c.FaseId)
            .OnDelete(DeleteBehavior.SetNull);

        // ─── ArchivoProyecto ──────────────────────────────────────────────────
        modelBuilder.Entity<ArchivoProyecto>()
            .HasOne(a => a.Proyecto)
            .WithMany()
            .HasForeignKey(a => a.ProyectoId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<ArchivoProyecto>()
            .Property(a => a.TipoDocumento)
            .HasConversion<string>();

        // ─── PagoCliente ──────────────────────────────────────────────────────
        modelBuilder.Entity<PagoCliente>()
            .HasOne(p => p.Proyecto)
            .WithMany()
            .HasForeignKey(p => p.ProyectoId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<PagoCliente>()
            .Property(p => p.Monto)
            .HasColumnType("decimal(18,2)");

        modelBuilder.Entity<Herramienta>()
            .Property(h => h.ValorAdquisicion)
            .HasColumnType("decimal(18,2)");

        // ─── Conversión string para enums ─────────────────────────────────────

        modelBuilder.Entity<Herramienta>()
            .Property(h => h.Estado)
            .HasConversion<string>();

        modelBuilder.Entity<AsignacionHerramienta>()
            .Property(a => a.Estado)
            .HasConversion<string>();

        modelBuilder.Entity<Proyecto>()
            .Property(p => p.Estado)
            .HasConversion<string>();

        modelBuilder.Entity<AlmacenCentral>()
            .Property(a => a.Zona)
            .HasConversion<string>();

        modelBuilder.Entity<AlmacenCentral>()
            .Property(a => a.TipoUbicacion)
            .HasConversion<string>();

        modelBuilder.Entity<EntradaDetalle>()
            .Property(e => e.Zona)
            .HasConversion<string>();

        modelBuilder.Entity<EntradaDetalle>()
            .Property(e => e.TipoUbicacion)
            .HasConversion<string>();

        modelBuilder.Entity<AlmacenProyecto>()
            .Property(a => a.Zona)
            .HasConversion<string>();

        modelBuilder.Entity<AlmacenProyecto>()
            .Property(a => a.TipoUbicacion)
            .HasConversion<string>();

        // ─── FaseProyecto ─────────────────────────────────────────────────────

        modelBuilder.Entity<FaseProyecto>()
            .Property(f => f.Estado)
            .HasConversion<string>();

        modelBuilder.Entity<FaseProyecto>()
            .HasOne(f => f.Proyecto)
            .WithMany()
            .HasForeignKey(f => f.ProyectoId)
            .OnDelete(DeleteBehavior.Cascade);

        // ─── RegistroPerdida ──────────────────────────────────────────────────

        modelBuilder.Entity<RegistroPerdida>()
            .Property(r => r.Tipo)
            .HasConversion<string>();

        modelBuilder.Entity<RegistroPerdida>()
            .Property(r => r.Motivo)
            .HasConversion<string>();

        modelBuilder.Entity<RegistroPerdida>()
            .Property(r => r.CantidadPerdida)
            .HasColumnType("decimal(18,2)");

        modelBuilder.Entity<RegistroPerdida>()
            .HasOne(r => r.Material)
            .WithMany()
            .HasForeignKey(r => r.MaterialId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<RegistroPerdida>()
            .HasOne(r => r.Herramienta)
            .WithMany()
            .HasForeignKey(r => r.HerramientaId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<RegistroPerdida>()
            .HasOne(r => r.Proyecto)
            .WithMany()
            .HasForeignKey(r => r.ProyectoId)
            .OnDelete(DeleteBehavior.SetNull);

        // ─── DevolucionMaterial ───────────────────────────────────────────────

        modelBuilder.Entity<DevolucionMaterial>()
            .Property(d => d.CantidadDevuelta)
            .HasColumnType("decimal(18,2)");

        modelBuilder.Entity<DevolucionMaterial>()
            .HasOne(d => d.Proyecto)
            .WithMany()
            .HasForeignKey(d => d.ProyectoId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<DevolucionMaterial>()
            .HasOne(d => d.Material)
            .WithMany()
            .HasForeignKey(d => d.MaterialId)
            .OnDelete(DeleteBehavior.Restrict);

        // ─── Índices únicos ───────────────────────────────────────────────────

        modelBuilder.Entity<Usuario>()
            .HasIndex(u => u.NombreUsuario).IsUnique();

        modelBuilder.Entity<Usuario>()
            .HasIndex(u => u.Email).IsUnique();

        modelBuilder.Entity<TokenSesion>()
            .HasIndex(t => t.Token).IsUnique();

        modelBuilder.Entity<Material>()
            .HasIndex(m => m.Codigo).IsUnique();

        modelBuilder.Entity<Herramienta>()
            .HasIndex(h => h.Codigo).IsUnique();

        modelBuilder.Entity<Herramienta>()
            .HasIndex(h => h.NumeroSerie).IsUnique();

        modelBuilder.Entity<Proveedor>()
            .HasIndex(p => p.RFC).IsUnique();

        modelBuilder.Entity<Cliente>()
            .HasIndex(c => c.RFC).IsUnique();

        modelBuilder.Entity<AlmacenCentral>()
            .HasIndex(a => a.MaterialId).IsUnique();

        modelBuilder.Entity<AlmacenProyecto>()
            .HasIndex(a => new { a.ProyectoId, a.MaterialId }).IsUnique();

        modelBuilder.Entity<UnidadMedida>()
            .HasIndex(u => u.Abreviatura).IsUnique();

        // ─── MFA Config ───────────────────────────────────────────────────────
        modelBuilder.Entity<UsuarioMfaConfig>()
            .HasIndex(u => u.UsuarioId).IsUnique();

        modelBuilder.Entity<MfaEmailCode>()
            .HasIndex(m => new { m.UsuarioId, m.Usado, m.FechaExpira });

        modelBuilder.Entity<MfaEmailCode>()
            .HasIndex(m => m.FechaExpira);

        modelBuilder.Entity<PasswordResetCode>()
            .HasIndex(p => new { p.UsuarioId, p.Usado, p.FechaExpira });

        modelBuilder.Entity<PasswordResetCode>()
            .HasIndex(p => p.FechaExpira);
    }
}