# Sistema Supermercado — Auth

Microservicio de **autenticación y usuarios**: usuarios, roles, menús, permisos e inicio de sesión. Es el **único
que emite tokens JWT**: los firma con RS256 y publica la clave pública (JWKS) para que los demás servicios los validen.

**Base de datos:** `DB_Auth` — `USUARIO`, `ROL`, `MENU`, `PERMISO`.

📘 Documentación:
- [Cómo levantar todo el sistema](https://github.com/AbrahamzzZ/Sistema-Supermercado-Backend-Gateway#readme) (README del Gateway)
- [Manual técnico](https://github.com/AbrahamzzZ/Sistema-Supermercado-Backend-Gateway/blob/main/docs/Manual%20Tecnico.docx): capítulo 5 (Seguridad) y 6 (Auth)

## Ejecutar

Normalmente se levanta con el `docker-compose.yml` del repo del Gateway (Swagger: http://localhost:8002/swagger).

Solo este servicio, desde Visual Studio o la terminal:

```bash
dotnet run --project Auth --launch-profile https      # https://localhost:7186/swagger
```

Necesita SQL Server (`ConnectionStrings:CadenaSQL`) y la clave privada. Para que la clave no cambie en cada reinicio:

```powershell
cd Auth
dotnet user-secrets init
dotnet user-secrets set "Jwt:PrivateKeyPath" "C:\ruta\Sistema-Supermercado-Backend-Gateway\secrets\jwt_private_key.pem"
```

Auth debe arrancar **antes** que los demás servicios, porque estos descargan su clave pública.

## Endpoints principales

| Ruta (vía Gateway `/auth/...`) | Descripción |
|---|---|
| `POST /api/Usuario/login` | Inicia sesión y devuelve el JWT (no requiere token) |
| `/api/Usuario` | CRUD de usuarios (borrado lógico) |
| `GET /api/Rol` · `GET /api/Menu/{idUsuario}` | Roles y menús del usuario |
| `GET /.well-known/jwks.json` | Clave pública para validar tokens |

> ⚠️ Nunca subas la clave privada (`*.pem`) al repositorio.
