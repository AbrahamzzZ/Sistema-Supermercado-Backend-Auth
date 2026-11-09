# Sistema Supermercado — Auth

Microservicio de **autenticación y usuarios**. Es el **único que emite tokens JWT**: los firma con RS256 usando una
clave privada y publica la clave pública (JWKS) para que el resto de servicios pueda validarlos.

> 📘 Para levantar **todo el sistema** (Docker, Kafka, Gateway, etc.) ver el
> [README principal en el repo del Gateway](https://github.com/AbrahamzzZ/Sistema-Supermercado-Backend-Gateway#readme).

## Responsabilidad

- Usuarios, roles, menús y permisos
- Login (`PA_INICIAR_SESION`) y generación del token JWT con los permisos del usuario como claims
- Publicación de la clave pública: `/.well-known/openid-configuration` y `/.well-known/jwks.json`

**Base de datos:** `DB_Auth` — tablas `USUARIO`, `ROL`, `MENU`, `PERMISO`.

## Estructura

```
Auth/            API (controllers, Program.cs, appsettings)
Domain/          Entidades, DbContext, configuraciones EF y Migrations
Infrastructure/  Repositorios, servicios, validadores y Helpers (Token.cs, RsaKeyProvider.cs)
Utilities/       ApiResponse, mensajes, paginación, encriptación
```

## Endpoints

Vía Gateway: `http://localhost:8000/auth/...` → aquí `/api/...`

| Método | Ruta | Token | Descripción |
|---|---|---|---|
| POST | `/api/Usuario/login` | No | Inicia sesión y devuelve el JWT |
| GET | `/api/Usuario` | Sí | Lista usuarios |
| GET | `/api/Usuario/paginacion?pageNumber=1&pageSize=10` | Sí | Lista paginada |
| GET | `/api/Usuario/{id}` | Sí | Obtiene un usuario |
| POST / PUT / DELETE | `/api/Usuario[/{id}]` | Sí | Registrar / editar / eliminar |
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

## Migraciones (EF Core Code First)

Las migraciones se aplican solas al iniciar (`Database.Migrate()` en `Program.cs`). Para crear una nueva:

```bash
dotnet ef migrations add NombreMigracion --project Domain --startup-project Auth
```

## Docker

```bash
docker build -t supermercado-auth .
```

Normalmente se levanta junto al resto con el `docker-compose.yml` del repo del Gateway.
