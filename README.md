# Sistema Supermercado — Auth

Microservicio de **autenticación y usuarios**. Es el **único que emite tokens JWT**: los firma con RS256 usando una
clave privada y publica la clave pública (JWKS) para que el resto de servicios pueda validarlos.

> 📘 Para levantar **todo el sistema** (Docker, Kafka, Gateway, etc.) ver el
> [README principal en el repo del Gateway](https://github.com/AbrahamzzZ/Sistema-Supermercado-Backend-Gateway#readme).

## Responsabilidad

- Usuarios, roles, menús y permisos
- Login y generación del token JWT con los permisos del usuario como claims
- Contraseñas con **BCrypt**
- Publicación de la clave pública: `/.well-known/openid-configuration` y `/.well-known/jwks.json`

**Base de datos:** `DB_Auth` — tablas `USUARIO`, `ROL`, `MENU`, `PERMISO`.

## Estructura

```
Auth/            API (controllers, Program.cs, appsettings) y Observabilidad/ (Correlation Id, errores globales)
Domain/          Entidades, DbContext, configuraciones EF y Migrations (+ Migrations/Scripts/*.sql)
Infrastructure/  Repositorios, servicios, validadores, Helpers (Token.cs, RsaKeyProvider.cs)
                 y Security/ (ICurrentUserService, ClaveHasher)
Utilities/       ApiResponse, mensajes, paginación, encriptación legada (SHA-256)
```

## Contraseñas (BCrypt)

- Al registrar o editar, la clave se guarda con **BCrypt** (work factor 11, con sal). Si al editar la clave viene vacía,
  se conserva la actual.
- El login ya **no compara la clave en SQL**: `PA_INICIAR_SESION` recibe solo el correo y devuelve el usuario con su hash;
  la verificación se hace en C# (`Infrastructure/Security/ClaveHasher.cs`).
- **Migración gradual:** los usuarios antiguos tienen la clave en SHA-256 (hex, sin sal). En su primer login correcto,
  la API verifica con el método antiguo y la vuelve a guardar con BCrypt (`PA_ACTUALIZAR_CLAVE`). No hace falta
  pedirles que cambien la contraseña.
- El hash nunca sale en las respuestas de la API (`UsuarioRol` no tiene la propiedad `Clave`).

## Auditoría y borrado lógico

`USUARIO` tiene las columnas de auditoría comunes a todos los microservicios:

| Columna | Se llena |
|---|---|
| `ID_USUARIO_CREACION` | Al registrar, con el usuario del token |
| `FECHA_CREACION` | Al registrar (UTC, la pone SQL Server con `SYSUTCDATETIME()`) |
| `ID_USUARIO_MODIFICACION` | Al editar o eliminar, con el usuario del token |
| `FECHA_MODIFICACION` | Al editar o eliminar (UTC) |
| `ELIMINADO` | `1` cuando se elimina |

- El usuario **siempre** sale del token JWT (`ICurrentUserService` lee el claim `NameIdentifier`), nunca del body.
- **Eliminar es lógico:** `DELETE /api/Usuario/{id}` marca `ELIMINADO = 1`. El registro se conserva porque Order guarda
  ese `ID_USUARIO` en compras y ventas. Listar, obtener, paginar y login ignoran los eliminados; un usuario eliminado
  no puede iniciar sesión.
- `ROL`, `MENU` y `PERMISO` son catálogos fijos (datos iniciales): no llevan auditoría.

## Endpoints

Vía Gateway: `http://localhost:8000/auth/...` → aquí `/api/...`

| Método | Ruta | Token | Descripción |
|---|---|---|---|
| POST | `/api/Usuario/login` | No | Inicia sesión y devuelve el JWT |
| GET | `/api/Usuario` | Sí | Lista usuarios (no eliminados) |
| GET | `/api/Usuario/paginacion?pageNumber=1&pageSize=10` | Sí | Lista paginada |
| GET | `/api/Usuario/{id}` | Sí | Obtiene un usuario |
| POST | `/api/Usuario` | Sí | Registra (clave con BCrypt) |
| PUT | `/api/Usuario/{id}` | Sí | Edita (el `id` de la ruta manda sobre el del body) |
| DELETE | `/api/Usuario/{id}` | Sí | Elimina (borrado lógico) |
| GET | `/api/Rol` | Sí | Lista roles |
| GET | `/api/Menu/{idUsuario}` | Sí | Menús del usuario |
| GET | `/.well-known/openid-configuration` | No | Documento de descubrimiento |
| GET | `/.well-known/jwks.json` | No | Clave pública para validar tokens |

Swagger: http://localhost:8002/swagger (Docker) · https://localhost:7186/swagger (local)

## Configuración

| Clave | Descripción |
|---|---|
| `ConnectionStrings:CadenaSQL` | Cadena de conexión a `DB_Auth` |
| `Jwt:PrivateKeyPath` | Ruta al `.pem` con la clave privada RSA (Docker: `/run/secrets/jwt_private_key`) |
| `Jwt:PrivateKey` | Alternativa: contenido PEM directo (para `dotnet user-secrets`) |
| `Jwt:Issuer` / `Jwt:Audience` | Deben coincidir con los del resto de servicios |
| `Jwt:DurationInMinutes` | Vigencia del token |
| `Serilog:MinimumLevel` | Nivel mínimo de logs (por defecto `Information`) |
| `Seq:ServerUrl` | URL de Seq (local: `http://localhost:5341`; Docker: `http://seq:80`) |

Si no se configura `PrivateKeyPath` ni `PrivateKey`, Auth genera una **clave temporal** y lo avisa en el log
(los tokens dejan de valer al reiniciar). Para desarrollo local:

```powershell
cd Auth
dotnet user-secrets init
dotnet user-secrets set "Jwt:PrivateKeyPath" "C:\ruta\a\Sistema-Supermercado-Backend-Gateway\secrets\jwt_private_key.pem"
```

> ⚠️ Nunca subas la clave privada (`*.pem`) al repositorio.

## Ejecutar solo este servicio

```bash
dotnet run --project Auth --launch-profile https
```

Auth debe arrancar **antes** que los demás servicios, porque estos descargan su clave pública al validar tokens.

## Logs (Serilog + Seq)

Los logs son **estructurados** y se envían a la consola y a **Seq** (servidor central de logs de todos los microservicios).
Cada evento lleva las propiedades `Servicio = Auth`, `CorrelationId` y, en el log de cada petición, `IdUsuario`.

| Nivel | Cuándo |
|---|---|
| `Information` | Todo salió bien (login exitoso, usuario registrado/editado/eliminado, cada petición 2xx) |
| `Warning` | Se incumplió una regla de negocio o validación (credenciales inválidas, usuario inactivo, código duplicado, único administrador, peticiones 4xx) |
| `Error` | Falla técnica: excepción no controlada o respuesta 5xx |

- **Correlation Id:** `CorrelationIdMiddleware` lee el header `X-Correlation-Id` (lo genera si no viene), lo agrega a
  todos los logs de la petición y lo devuelve en la respuesta. Así se sigue una petición de punta a punta en Seq.
- **Errores no controlados:** `GlobalExceptionHandler` los registra como `Error` y responde `500` con el formato
  `ApiResponse` y el código de seguimiento (`Ocurrió un error interno. Código de seguimiento: {correlationId}`),
  sin exponer el stack trace.
- Nunca se registran contraseñas ni tokens.

## Migraciones (EF Core Code First)

Las migraciones se aplican solas al iniciar (`Database.Migrate()` en `Program.cs`).

| Migración | Contenido |
|---|---|
| `Init` | Tablas `USUARIO`, `ROL`, `MENU`, `PERMISO` |
| `AuditoriaYProcedimientos` | Columnas de auditoría y `ELIMINADO` en `USUARIO`, fechas en UTC (`datetime2`), procedimientos almacenados y datos iniciales |

### Procedimientos almacenados y datos iniciales

Los SP y los datos iniciales están en archivos `.sql` dentro de `Domain/Migrations/Scripts/`, **embebidos** en el
ensamblado `Domain` (ver `Domain.csproj`). La migración los ejecuta con `migrationBuilder.EjecutarScript("archivo.sql")`
(`ScriptRunner.cs`), que divide cada archivo por las líneas `GO`, igual que SSMS.

| Archivo | Contenido |
|---|---|
| `V1_01_Procedimientos_Rol_Menu.sql` | `PA_LISTA_ROL`, `PA_OBTENER_MENU` |
| `V1_02_Procedimientos_Usuario.sql` | `PA_LISTA_USUARIO`, `PA_LISTA_USUARIO_PAGINACION`, `PA_OBTENER_USUARIO`, `PA_INICIAR_SESION`, `PA_ACTUALIZAR_CLAVE`, `PA_REGISTRAR_USUARIO`, `PA_EDITAR_USUARIO`, `PA_ELIMINAR_USUARIO` |
| `V1_03_Datos_Iniciales.sql` | Roles, menús, permisos y usuario administrador (idempotente) |

**Para cambiar un SP en el futuro:** no edites un archivo que ya se aplicó (las bases existentes no lo volverían a
ejecutar). Crea un archivo nuevo versionado, por ejemplo `V2_01_Usuario_NuevoCampo.sql`, con `CREATE OR ALTER PROCEDURE`,
y una migración nueva que lo ejecute:

```bash
dotnet ef migrations add UsuarioNuevoCampo --project Domain --startup-project Auth
# en el Up() de la migración generada:
#   migrationBuilder.EjecutarScript("V2_01_Usuario_NuevoCampo.sql");
```

### Datos iniciales

- Roles: `Administrador` (1) y `Empleado` (2), con sus menús y permisos.
- **Usuario administrador:** `admin@supermercado.com` / `Admin123*`. Se crea con hash SHA-256 (SQL Server no puede
  generar BCrypt) y se convierte a BCrypt en el primer login. **Cámbiale la clave después del primer ingreso.**

## Docker

```bash
docker build -t supermercado-auth .
```

Normalmente se levanta junto al resto con el `docker-compose.yml` del repo del Gateway.
