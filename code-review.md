# Revisión de Código — BLL-ConstruccionAPI

## 1. Estructura del Proyecto

El proyecto organiza el código en las capas esperadas de ASP.NET Core. La estructura es coherente en general, pero hay algunos problemas notables con los nombres de carpetas y espacios de nombres:

- **`Models/Inventario/Cátalogos/`** — La `á` acentuada en el nombre de la carpeta es técnicamente válida, pero puede causar problemas en sistemas de archivos de Linux (por ejemplo, en CI/CD o Docker) donde los nombres de archivo con acentos pueden no funcionar correctamente.
- **`Models/Inventario/Proveedores y Clientes/`** — Los espacios en el nombre de la carpeta no son estándar y pueden causar problemas con herramientas sensibles a rutas.
- **`Models/Inventario/Proyecto/`** (carpeta) vs. `namespace BLL_ConstruccionAPI.Models.Inventario.Proyectos` (espacio de nombres) — La carpeta está en singular pero el espacio de nombres en plural (`Proyectos`). Esta inconsistencia no afecta la compilación pero es confusa.
- **`Models/Inventario/Materiales/Provedoor.cs`** — El nombre del archivo `Provedoor.cs` tiene un error tipográfico (doble `o`). La clase dentro se llama `Proveedor` (correcta), pero el nombre del archivo está mal.

---

## 2. Patrón de Arquitectura

El patrón Repositorio + Servicio se aplica de forma **consistente** en todos los módulos. Cada módulo tiene:
- Una interfaz de repositorio en `Repositories/Interfaces/`
- Una implementación de repositorio en `Repositories/`
- Una interfaz de servicio en `Services/Interfaces/`
- Una implementación de servicio en `Services/`
- Un controlador en `Controllers/`

Esto está bien implementado. Los registros de inyección de dependencias en `Program.cs` (líneas 18–47) son completos y correctos.

---

## 3. DTOs

### ✅ Lo que funciona
- Los DTOs de solicitud están definidos para cada operación de creación/actualización.
- Los DTOs de autenticación (`LoginRequestDto`, `RegisterRequestDto`, etc.) están bien estructurados.

### ❌ Problemas Críticos

**No existen DTOs de respuesta.** Cada endpoint GET en cada interfaz de servicio devuelve la entidad de dominio directamente (p. ej., `Task<IEnumerable<Material>>`, `Task<Proyecto?>`, `Task<IEnumerable<Herramienta>>`). Esto expone el modelo de persistencia interno, puede generar ciclos en la serialización JSON por las propiedades de navegación (`Categoria → Material → Categoria...`), y filtra IDs internos y marcas de tiempo a los clientes. Cada módulo necesita un `*ResponseDto` correspondiente.

**Ningún DTO tiene atributos de validación de datos.** Ninguno de los DTOs de solicitud tiene `[Required]`, `[StringLength]`, `[Range]`, `[EmailAddress]` ni ningún otro atributo de validación. La validación solo ocurre en la capa de servicio mediante comprobaciones manuales de cadenas, ignorando el pipeline de validación estándar de ASP.NET Core. Por ejemplo:
- `MaterialRequestDto` — `Nombre` y `Codigo` pueden ser cadenas vacías
- `LoginRequestDto` — `NombreUsuario` y `Password` no tienen `[Required]`
- `RegisterRequestDto` — `Email` no tiene `[EmailAddress]`

**`UsuarioId` se toma del cuerpo de la solicitud, no de los claims del JWT.** Esto es una vulnerabilidad de seguridad (ver §10):
- `EntradaRequestDto.UsuarioId` (línea 7)
- `SalidaRequestDto.UsuarioId` (línea 7)
- `AsignacionRequestDto.UsuarioAsignoId` (línea 7)

---

## 4. Capa de Repositorio

### ✅ Lo que funciona
- Todos los repositorios inyectan correctamente `AppDbContext` y usan `async`/`await` en todas partes.
- El patrón de borrado suave (establecer `Activo = false`) se usa de forma consistente en `CatalogosRepository`, `MaterialesRepository`, `ProveedoresClientesRepository` y `HerramientasRepository`.
- `EntradasRepository` y `SalidasRepository` usan un único `SaveChangesAsync` por transacción aprovechando el `DbContext` de ámbito compartido — es un enfoque inteligente, pero conlleva riesgos (ver más abajo).

### ❌ Problemas

**No hay restricción de unicidad en la BD para `AlmacenCentral`.** `AlmacenCentral` no tiene un índice único en `MaterialId`. Si se insertan accidentalmente dos registros para el mismo material, `GetStockCentralAsync` (que usa `FirstOrDefaultAsync`) devolverá solo uno silenciosamente, dejando el otro obsoleto.

**El patrón de "transacción atómica con Change Tracker compartido" es frágil.** `EntradasRepository.RegistrarEntradaAsync` (líneas 39–44) depende de que `AlmacenCentral` ya haya sido modificado en memoria por `EntradasService` antes de la llamada. Esto funciona porque ambos repositorios comparten el mismo `AppDbContext` de ámbito, pero:
- No hay un `IDbContextTransaction` explícito.
- La práctica más segura es usar un bloque `await _context.Database.BeginTransactionAsync()`.

**`EntradasService.cs` línea 72 — Stock no actualizado en silencio:**
```csharp
var stockCentral = await _materialesRepo.GetStockCentralAsync(item.MaterialId);
if (stockCentral is not null)
{
    stockCentral.Stock += item.Cantidad;
    ...
}
```
Si `stockCentral` es `null` (lo que no debería ocurrir, pero podría si el registro de `AlmacenCentral` no existe), la compra se registra en la tabla `Entradas` pero **el inventario nunca se actualiza**. No se lanza ningún error. Esto debería ser un error duro: `return (false, $"No existe registro de almacén central para el material ID {item.MaterialId}.", null);`

**`IEntradasRepository` no tiene un método `DeleteAsync` / cancelar-entrada.** Una vez registrada una `Entrada`, no hay forma de revertirla. Puede ser intencional, pero debería estar documentado.

**`IMaterialesRepository` no tiene `GetAllAlmacenCentralAsync`.** No hay forma de consultar todos los niveles de stock en una sola llamada a través del repositorio. Solo existen consultas por material.

---

## 5. Capa de Servicio

### ✅ Lo que funciona
- La validación de entradas en el servicio es exhaustiva: verificación de existencia de FK antes de crear, verificación de claves duplicadas, verificación de stock antes de salidas.
- `HerramientasService.AsignarAsync` valida correctamente que la herramienta esté "Disponible" antes de asignarla.
- `SalidasService.RegistrarAsync` comprueba correctamente `stockCentral.Stock < item.Cantidad` antes de descontar.

### ❌ Errores y Problemas

**`HerramientasService.DevolverAsync` no es atómica (líneas 154–164):**
```csharp
asignacion.Estado = "Devuelta";
...
await _herramientasRepo.UpdateAsignacionAsync(asignacion);  // SaveChanges #1

var herramienta = await _herramientasRepo.GetByIdAsync(asignacion.HerramientaId);
if (herramienta is not null)
{
    herramienta.Estado = "Disponible";
    await _herramientasRepo.UpdateAsync(herramienta);  // SaveChanges #2
}
```
Dos llamadas separadas a `SaveChangesAsync`. Si la segunda falla, `AsignacionHerramienta.Estado` quedará como "Devuelta" pero `Herramienta.Estado` seguirá como "Asignada". La herramienta quedará bloqueada y no podrá reasignarse. Ambas actualizaciones deben estar en un único `SaveChangesAsync`.

**`HerramientasService.AsignarAsync` tiene el mismo problema de dos guardados (líneas 138–141):**
```csharp
await _herramientasRepo.CreateAsignacionAsync(asignacion); // SaveChanges #1
herramienta.Estado = "Asignada";
await _herramientasRepo.UpdateAsync(herramienta);           // SaveChanges #2
```
Si el segundo guardado falla, la asignación existe pero la herramienta sigue mostrando "Disponible", permitiendo una doble asignación.

**`SalidasService.RegistrarAsync` no valida el estado del proyecto.** Las líneas 44–46 solo verifican que el proyecto exista, no que `proyecto.Estado == "Activo"`. Los materiales pueden ser distribuidos a un proyecto "Terminado" o "Pausado".

**Las semánticas de `ProyectosService.DeleteAsync` son engañosas.** Establece `Estado = "Terminado"` y devuelve "Proyecto marcado como Terminado." Pero el verbo HTTP es `DELETE` y el método es `DeleteAsync`. Una operación DELETE debe significar semánticamente eliminación (o al menos borrado suave con `Activo = false`). Debería ser un endpoint dedicado `PUT /api/proyectos/{id}/terminar` o cambiar el comportamiento.

**`AuthService.Enable2FAAsync` permite que cualquier llamante sobrescriba la clave 2FA de cualquier usuario.** El endpoint acepta un `userId` del cuerpo de la solicitud sin ninguna verificación de autenticación. Cualquier llamante no autenticado puede llamar a `POST /api/auth/2fa/enable` con `123` en el cuerpo y sobrescribir el secreto TOTP del usuario 123.

---

## 6. Controladores

### 🔴 CRÍTICO: Ningún Controlador tiene `[Authorize]`

**Absolutamente ningún controlador tiene el atributo `[Authorize]`.** El middleware JWT está registrado en `Program.cs` (líneas 51–64) y se llama a `UseAuthentication()` (línea 79), pero **sin atributos `[Authorize]`, todos los endpoints son de acceso público.** Esto invalida completamente la autenticación JWT.

Controladores afectados:
- `AuthController` — los endpoints de auth deben permanecer abiertos, pero `2fa/enable` y `2fa/verify` deberían requerir autenticación
- `EntradaController`, `SalidaController`, `MaterialController`, `HerramientaController` — todos los endpoints operacionales deben estar protegidos
- `ProyectoController`, `ProveedorController`, `ClienteController` — igual
- `CategoriaMaterialController`, `CategoriaHerramientaController`, `UnidadMedidaController` — igual

La solución es agregar `[Authorize]` a nivel de controlador en todos los controladores que no sean de autenticación, y `[AllowAnonymous]` en los endpoints específicos de auth que deben ser públicos (`/register`, `/login`, `/login/2fa`, `/refresh`).

### Otros Problemas en Controladores

**`AuthController.cs` línea 73 — `[FromBody] int usuarioId` es frágil:**
```csharp
[HttpPost("2fa/enable")]
public async Task<IActionResult> Enable2FA([FromBody] int usuarioId)
```
Este binding requiere que el cuerpo JSON sea un número sin procesar: `123`. Fallará si el cliente envía `{"usuarioId": 123}`. Se debe usar un DTO dedicado.

**Inconsistencias en códigos de estado HTTP:**

| Controlador | Acción | Problema |
|---|---|---|
| `HerramientaController` | `Delete` (líneas 71–75) | Devuelve `BadRequest` para "no encontrado" — debería ser `NotFound` |
| `HerramientaController` | `Asignar` (líneas 82–85) | Devuelve `200 OK` — debería ser `201 Created` |
| `ProveedorController` | `Update` (líneas 46–49) | Devuelve `BadRequest` para un proveedor no encontrado — el mensaje es indistinguible de un error de validación |
| `MaterialController` | `Update` (líneas 62–68) | Igual, devuelve `BadRequest` para no encontrado |

**Ninguna acción tiene atributos `[ProducesResponseType]`.** Swagger no puede generar esquemas de respuesta precisos.

**Sin manejo global de excepciones.** Una excepción no controlada (p. ej., fallo de conexión a la BD) devolverá un 500 con el stack trace completo al cliente en desarrollo, y un 500 vacío en producción.

---

## 7. AppDbContext

### ✅ Lo que funciona
- Los tipos de columna decimal están correctamente configurados para todas las propiedades `decimal` (líneas 86–124).
- Los ciclos de eliminación en cascada se rompen explícitamente para `Proyecto`, `Entrada`, `Salida` y `AsignacionHerramienta` (líneas 52–83).

### ❌ Problemas

**No se definen índices únicos en `OnModelCreating`.** Todas las verificaciones de unicidad se hacen en el código de la aplicación (`ExisteCodigoAsync`, `ExisteNombreUsuarioAsync`, etc.). Sin restricciones únicas a nivel de base de datos, existen condiciones de carrera — dos solicitudes concurrentes podrían pasar ambas la verificación de unicidad y crear entradas duplicadas. Índices faltantes:

| Entidad | Campo(s) Únicos |
|---|---|
| `Usuario` | `NombreUsuario`, `Email` |
| `Material` | `Codigo` |
| `Herramienta` | `Codigo`, `NumeroSerie` |
| `Proveedor` | `RFC` |
| `Cliente` | `RFC` |
| `AlmacenCentral` | `MaterialId` |
| `AlmacenProyecto` | `(ProyectoId, MaterialId)` |
| `UnidadMedida` | `Abreviatura` |
| `TokenSesion` | `Token` |

**`Usuario.RolId` no tiene relación FK configurada.** La tabla `Rol` existe como `DbSet<Rol>` pero no hay configuración `HasOne`/`WithMany` o `HasForeignKey` para `Usuario.RolId`. EF Core solo inferirá una FK sombra por convención si existe la propiedad de navegación. Como `Usuario` no tiene `Rol? Rol` (líneas 10–13 de `Usuario.cs`), no hay navegación para consultas basadas en roles ni se aplica una restricción FK en la base de datos.

**Los modelos de auth no tienen propiedades de navegación ni configuraciones FK:**
- `TokenSesion.UsuarioId` — sin navegación a `Usuario`, sin configuración FK
- `Usuario2FA.UsuarioId` — igual
- `LogAcceso.UsuarioId` — igual

**`GetCategoriaByIdAsync` (y todos los Gets basados en `FindAsync`) devuelven entidades con borrado suave.** `FindAsync(id)` no filtra por `Activo`. Un llamante puede recuperar una categoría inactiva por ID.

---

## 8. Program.cs

### ✅ El orden del middleware es correcto
`UseAuthentication()` (línea 79) se llama antes que `UseAuthorization()` (línea 80). ✅

### ❌ Problemas

**El secreto JWT está confirmado en el control de versiones** (`appsettings.json` línea 7):
```json
"SecretKey": "C0nstruct0r4-Inv3ntar10-S3cr3t-K3y-2026!"
```
Los secretos nunca deben estar en `appsettings.json` confirmado en un repositorio. Usar `dotnet user-secrets` para desarrollo y variables de entorno (o Azure Key Vault) para producción.

**`builder.Configuration["Jwt:SecretKey"]!` (Program.cs línea 50).** El operador `!` de supresión de nulos significa que si la clave falta en la configuración, la aplicación fallará con `NullReferenceException` en lugar de un error de configuración claro. Usar `ArgumentNullException.ThrowIfNullOrWhiteSpace(jwtKey, "Jwt:SecretKey")`.

**Swagger no está configurado con autenticación JWT bearer.** `AddSwaggerGen()` (línea 68) no tiene `AddSecurityDefinition` / `AddSecurityRequirement`. La UI de Swagger no puede usarse para probar endpoints protegidos.

**Sin política CORS.** Si esta API sirve a un frontend web desde un origen diferente, CORS bloqueará todas las solicitudes.

**Sin limitación de velocidad.** Los endpoints de autenticación (login, register) no tienen throttling, haciéndolos vulnerables a ataques de fuerza bruta.

---

## 9. Modelos

### Auth
- `Usuario` (líneas 5–13) — Falta la propiedad de navegación `public Rol? Rol { get; set; }`. No se puede incluir información del rol sin un join explícito.
- `LogAcceso.UsuarioId` — ¿Debería ser nullable para registrar intentos de login fallidos con nombres de usuario inexistentes? Actualmente, `AuthService.LoginAsync` (líneas 56–63) solo registra cuando `usuario is not null`, por lo que los intentos fallidos con usuarios desconocidos no se registran en absoluto.

### Inventario
- `Herramienta.Estado` (línea 13) — Usa una cadena mágica con un comentario `// Disponible, Asignada, Mantenimiento, Baja`. Debería ser un `public enum EstadoHerramienta`. Lo mismo aplica para `Proyecto.Estado` y `AsignacionHerramienta.Estado`.
- `EntradaDetalle` no tiene propiedad de navegación de regreso a `Entrada` (solo hacia adelante vía `Material`). EF Core infiere la FK de `EntradaId` por convención, pero el modelo está incompleto.
- Igual para `SalidaDetalle` — sin navegación de regreso a `Salida`.
- Namespace de `Proveedor.cs` es `BLL_ConstruccionAPI.Models.Inventario` pero vive en la subcarpeta `Models/Inventario/Proveedores y Clientes/`. El nombre del archivo también es `Provedoor.cs` (error tipográfico).

---

## 10. Resumen de Seguridad

| Problema | Severidad | Ubicación |
|---|---|---|
| Sin `[Authorize]` en ningún controlador | 🔴 Crítico | Todos los controladores |
| Secreto JWT en control de versiones | 🔴 Crítico | `appsettings.json:7` |
| `UsuarioId` tomado del cuerpo de la solicitud | 🔴 Crítico | `EntradaRequestDto`, `SalidaRequestDto`, `AsignacionRequestDto` |
| `Enable2FA` no requiere auth + usa userId del cuerpo | 🔴 Crítico | `AuthController:73`, `AuthService:160` |
| Sin restricciones únicas en BD (condición de carrera en creaciones) | 🟠 Alto | `AppDbContext` |
| Sin atributos de validación en DTOs | 🟠 Alto | Todos los DTOs |
| Sin limitación de velocidad en endpoints de auth | 🟠 Alto | `Program.cs` |
| Modelos de dominio devueltos directamente por la API (posible ref. circular) | 🟡 Medio | Todos los endpoints GET |
| Swagger sin configuración JWT | 🟡 Medio | `Program.cs:68` |
| Sin manejador global de excepciones | 🟡 Medio | `Program.cs` |
| RolId en JWT pero sin política de autorización basada en roles | 🟡 Medio | `AuthService:264` |

---

## 11. Lógica de Gestión de Stock

El flujo de stock está bien concebido:
- **Entrada** → incrementa `AlmacenCentral.Stock`
- **Salida** → decrementa `AlmacenCentral.Stock` + incrementa/crea `AlmacenProyecto.Stock`

El patrón de "rastrear y guardar juntos" (usando el `DbContext` de ámbito compartido entre repos) hace que cada transacción sea atómica correctamente. Sin embargo:

1. **Null silencioso en AlmacenCentral** — `EntradasService.cs:72-76`: Si `GetStockCentralAsync` devuelve `null`, la Entrada se guarda pero el stock nunca se actualiza. Esto es un escenario de corrupción de datos.
2. **Sin verificación del estado del proyecto** — `SalidasService.cs:44-46`: Los materiales pueden ser distribuidos a un proyecto `Terminado`.
3. **Sin transacción para operaciones de herramientas** — `AsignarAsync` y `DevolverAsync` usan dos llamadas separadas a `SaveChangesAsync`, que no son atómicas.
4. **Unicidad de AlmacenCentral** — Sin restricción única en `MaterialId` en `AlmacenCentral`, podrían acumularse filas duplicadas silenciosamente.

---

## 12. Módulos Faltantes

**Todos los módulos críticos de inventario existen** — `EntradaController`, `SalidaController`, y todos sus servicios y repositorios están presentes e implementados. No falta ningún módulo completo.

---

## 13. Posibles Errores Adicionales

1. **`AuthController.cs:73` — `[FromBody] int usuarioId`**: Requiere que el cuerpo JSON sea un literal entero sin procesar (p. ej., `42`). Si un cliente envía `{"usuarioId": 42}`, devolverá 400. Usar una clase DTO.

2. **`ProveedoresClientesRepository.GetProveedorByIdAsync`** devuelve un proveedor inactivo (usa `FindAsync`, sin filtrar por `Activo`). `EntradasService.cs:38-40` verifica explícitamente `!proveedor.Activo`, lo cual es correcto, pero `ProveedorController.GetById` devolvería un proveedor inactivo al cliente.

3. **`CatalogosRepository.ExisteCategoriaAsync`** (líneas 24–25) compara `c.Nombre == nombre` con comparación sensible a mayúsculas/minúsculas por defecto en C#. Dos categorías llamadas "Cemento" y "cemento" se considerarían diferentes a nivel de aplicación pero podrían ser iguales en SQL Server dependiendo de la intercalación.

4. **`ProyectosRepository.DeleteAsync`** (líneas 46–50): Establece `Estado = "Terminado"` pero no establece `Activo = false`. A diferencia de todos los demás borrados suaves en el codebase, este no usa el patrón del campo `Activo`. `GetAllAsync` no filtra por `Activo` para proyectos, por lo que los proyectos "eliminados" seguirán apareciendo en los listados.

5. **`LogoutAsync` (AuthService:153-157)**: No valida si el refresh token no está vacío antes de llamar a `RevocarTokenAsync`. Una búsqueda de token con cadena vacía contra la base de datos es inofensiva pero innecesaria y puede ser abusada.

6. **`RefreshTokenAsync` (AuthService:137-140)**: Los tokens expirados se acumulan indefinidamente en la tabla `TokensSesion` sin ningún proceso de limpieza.

---

## 14. Mejoras de Buenas Prácticas

1. **Agregar `[Authorize]` a todos los controladores que no son de auth inmediatamente** — Esta es la corrección más crítica.
2. **Extraer `UsuarioId` de `HttpContext.User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value`** en los controladores en lugar de aceptarlo del cuerpo de la solicitud.
3. **Crear DTOs de Respuesta** para cada entidad para desacoplar el contrato de la API del modelo de persistencia y prevenir serialización circular.
4. **Agregar anotaciones de datos** a todos los DTOs de solicitud (`[Required]`, `[StringLength(100, MinimumLength = 1)]`, `[Range(0.01, double.MaxValue)]`, `[EmailAddress]`).
5. **Mover el secreto JWT a variables de entorno** (`ASPNETCORE_JWT__SECRETKEY` o `dotnet user-secrets`).
6. **Configurar Swagger con JWT bearer** — agregar `AddSecurityDefinition` / `AddSecurityRequirement` en `AddSwaggerGen`.
7. **Agregar índices únicos** en `OnModelCreating` para todas las columnas de clave natural.
8. **Usar transacciones de BD explícitas** para operaciones de múltiples pasos (`AsignarAsync`, `DevolverAsync`, `RegistrarEntradaAsync`) en lugar de depender del patrón de Change Tracker compartido.
9. **Reemplazar estados con cadenas mágicas por enumeraciones** (`EstadoHerramienta`, `EstadoProyecto`, `EstadoAsignacion`).
10. **Agregar middleware de manejo global de excepciones** — `app.UseExceptionHandler("/error")` o middleware personalizado para capturar excepciones no controladas y devolver una respuesta JSON limpia.
11. **Agregar propiedad de navegación `Rol? Rol` a `Usuario`** y configurar la FK en `OnModelCreating`.
12. **Agregar endpoint `[HttpPut("{id}/terminar")]`** en lugar de sobrecargar `DELETE` con un comportamiento de "terminar" para proyectos.
13. **Agregar `AsNoTracking()` a consultas de solo lectura** (todos los `GetAllAsync`, `GetByIdAsync` que no van seguidos de actualizaciones) para mejorar el rendimiento.
14. **Corregir `ProyectosRepository.DeleteAsync`** para que coincida con el patrón de borrado suave usado en el resto del codebase (establecer `Activo = false` además de o en lugar de establecer `Estado`).
