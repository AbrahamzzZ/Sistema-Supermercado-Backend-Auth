# Sistema Supermercado — Auth

Microservicio de **autenticación y usuarios**: usuarios, roles, menús, permisos e inicio de sesión. Es el **único
que emite tokens JWT**: los firma con RS256 y publica la clave pública (JWKS) para que los demás servicios los validen.

**Base de datos:** `DB_Auth` — `USUARIO`, `ROL`, `MENU`, `PERMISO`.

📘 Documentación:
- [Cómo levantar todo el sistema](https://github.com/AbrahamzzZ/Sistema-Supermercado-Backend-Gateway#readme) (README del Gateway)
- [Manual técnico](https://github.com/AbrahamzzZ/Sistema-Supermercado-Backend-Gateway/blob/main/docs/Manual%20Tecnico.docx): capítulo 5 (Seguridad) y 6 (Auth)

## Ejecutar

Normalmente se levanta con el `docker-compose.yml` del repo del Gateway (Swagger: http://localhost:8002/swagger).

**Para depurar:** a diferencia de los demás servicios, **no detengas el contenedor `auth-api`**: el Gateway y los
otros servicios descargan desde él la clave pública para validar los tokens. Corre tu Auth local en paralelo:

1. Usa la misma clave privada que Docker, para que tus tokens sean válidos en todo el sistema (solo la primera vez):

   ```powershell
   cd Auth
   dotnet user-secrets init
   dotnet user-secrets set "Jwt:PrivateKeyPath" "C:\ruta\Sistema-Supermercado-Backend-Gateway\secrets\jwt_private_key.pem"
   ```

2. F5 en Visual Studio (o `dotnet run --project Auth --launch-profile https`) → https://localhost:7186/swagger.
   En el entorno Development, `appsettings.Development.json` lo conecta al SQL Server de Docker (`localhost,14330`,
   la misma `DB_Auth`).

Pruébalo desde su Swagger. Para usar tu propio SQL Server, define `ConnectionStrings:CadenaSQL` con
`dotnet user-secrets`.

## Endpoints principales

| Ruta (vía Gateway `/auth/...`) | Descripción |
|---|---|
| `POST /api/Usuario/login` | Inicia sesión y devuelve el JWT (no requiere token) |
| `/api/Usuario` | CRUD de usuarios (borrado lógico) |
| `GET /api/Rol` · `GET /api/Menu/{idUsuario}` | Roles y menús del usuario |
| `GET /.well-known/jwks.json` | Clave pública para validar tokens |

> ⚠️ Nunca subas la clave privada (`*.pem`) al repositorio.
