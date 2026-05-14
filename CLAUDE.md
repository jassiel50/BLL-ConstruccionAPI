# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Commands

```bash
dotnet build                                      # Compilar
dotnet run                                        # Ejecutar (http://localhost:5235)
dotnet ef migrations add <NombreMigracion>        # Crear migración
dotnet ef migrations remove                       # Eliminar última migración
dotnet ef database update                         # Aplicar migraciones manualmente
```

La app aplica migraciones pendientes automáticamente al iniciar (`db.Database.Migrate()` en `Program.cs`).

Swagger siempre disponible en `/swagger` (dev y prod).

## Configuración requerida

Configurar mediante `dotnet user-secrets` o variables de entorno:

- `ConnectionStrings:DefaultConnection` — cadena SQL Server
- `Jwt:SecretKey` — clave secreta para firmar JWT (requerido, la app falla al iniciar sin él)
- Credenciales de Resend para email (en `ResendEmailService`)

## Arquitectura

API REST en ASP.NET Core (.NET 10) para gestión de inventario y proyectos de construcción.

### Capas

```
Controllers → Services → Repositories → AppDbContext (EF Core / SQL Server)
```

- **Controllers** (`Controllers/`): Delegan toda la lógica a sus servicios. La validación de rol se hace manualmente con `User.FindFirstValue("rolId")` (no con `[Authorize(Roles)]`).
- **Services** (`Services/`): Lógica de negocio. Cada dominio tiene su interfaz en `Services/Interfaces/`.
- **Repositories** (`Repositories/`): Acceso a datos vía EF Core. Cada repositorio tiene su interfaz en `Repositories/Interfaces/`.
- **DTOs** (`DTOs/`): Modelos de entrada/salida por dominio; nunca se exponen entidades directamente.
- **Models** (`Models/`): Entidades EF Core organizadas por dominio.

### Dominios

| Dominio | Modelos clave |
|---|---|
| Auth | `Usuario`, `Rol`, `TokenSesion`, `UsuarioMfaConfig`, `MfaEmailCode`, `PasswordResetCode` |
| Catálogos | `CategoriaMaterial`, `CategoriaHerramienta`, `UnidadMedida` |
| Proveedores/Clientes | `Proveedor`, `Cliente` (RFC único) |
| Proyectos | `Proyecto`, `FaseProyecto`, `GastoExtra` |
| Materiales | `Material`, `AlmacenCentral`, `AlmacenProyecto`, `Entrada`/`EntradaDetalle`, `Salida`/`SalidaDetalle`, `DevolucionMaterial` |
| Herramientas | `Herramienta`, `AsignacionHerramienta` |
| Pérdidas | `RegistroPerdida` (material o herramienta) |
| Bitácora | `BitacoraActividad`, `LogAcceso` |
| Reportes | PDFs generados con QuestPDF (licencia Community) |

### Flujo de autenticación

1. `POST /api/auth/login` → valida credenciales → devuelve `MfaTicket` (JWT de corta duración)
2. El cliente elige método MFA (`app` TOTP o `email`) y envía el código
3. Al verificar correctamente → se emiten `AccessToken` (30 min) y `RefreshToken` (45 min) almacenado en BD
4. `POST /api/auth/refresh` rota el refresh token (el anterior se revoca)

El JWT incluye claims: `sub` (userId), `email`, `nombreUsuario`, `rolId`.  
Rol 1 = Administrador. Los roles se verifican manualmente en cada endpoint.

### Convenciones importantes

- **Enums guardados como strings** en la BD (`HasConversion<string>()`): `EstadoProyecto`, `EstadoFase`, `EstadoHerramienta`, `EstadoAsignacion`, `Zona`, `TipoUbicacion`, `TipoPerdida`, `MotivoPerdida`.
- **`ExpiredCodesCleanupService`**: hosted service en background que limpia códigos MFA y reset expirados.
- **CORS** configurado para `localhost:7284` (frontend local) y `app.bll.com.mx` (producción).
- **Namespace raíz**: `BLL_ConstruccionAPI` (con guion bajo, no guion).
