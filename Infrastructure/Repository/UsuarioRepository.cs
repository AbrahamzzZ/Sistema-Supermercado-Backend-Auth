using Domain.Context;
using Domain.Model;
using Domain.Model.Dto;
using Infrastructure.Repository.InterfacesRepository;
using Infrastructure.Security;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Data;
using Utilities.Shared;

namespace Infrastructure.Repository
{
    public class UsuarioRepository :IUsuarioRepository
    {
        private readonly AuthSistemaSupermercadoContext _context;
        private readonly ICurrentUserService _currentUser;

        public UsuarioRepository(AuthSistemaSupermercadoContext context, ICurrentUserService currentUser)
        {
            _context = context;
            _currentUser = currentUser;
        }

        private SqlParameter ParametroUsuario(string nombre) => new(nombre, (object?)_currentUser.IdUsuario ?? DBNull.Value);

        public async Task<List<UsuarioRol>> ListarUsuariosAsync()
        {
            return await _context.UsuariosDto
                .FromSqlRaw("EXEC PA_LISTA_USUARIO")
                .ToListAsync();
        }

        public async Task<Paginacion<UsuarioRol>> ListarUsuariosPaginacionAsync(int pageNumber, int pageSize)
        {

            using var command = _context.Database.GetDbConnection().CreateCommand();
            command.CommandText = "PA_LISTA_USUARIO_PAGINACION";
            command.CommandType = CommandType.StoredProcedure;
            command.Parameters.Add(new SqlParameter("@PageNumber", pageNumber));
            command.Parameters.Add(new SqlParameter("@PageSize", pageSize));

            await _context.Database.OpenConnectionAsync();

            var usuarios = new List<UsuarioRol>();
            int totalCount = 0;

            using (var reader = await command.ExecuteReaderAsync())
            {
                while (await reader.ReadAsync())
                {
                    usuarios.Add(new UsuarioRol
                    {
                        Id_Usuario = reader.GetInt32(0),
                        Codigo = reader.GetString(1),
                        Nombre_Completo = reader.GetString(2),
                        Correo_Electronico = reader.GetString(3),
                        Id_Rol = reader.GetInt32(4),
                        Nombre_Rol = reader.GetString(5),
                        Estado = reader.GetBoolean(6),
                        Fecha_Creacion = reader.GetDateTime(7)
                    });
                }

                if (await reader.NextResultAsync() && await reader.ReadAsync())
                {
                    totalCount = reader.GetInt32(0);
                }
            }

            return new Paginacion<UsuarioRol> { Items = usuarios, TotalCount = totalCount, PageNumber = pageNumber, PageSize = pageSize };
        }

        public async Task<UsuarioRol?> ObtenerUsuarioAsync(int idUsuario)
        {
            var idParam = new SqlParameter("@Id_Usuario", idUsuario);
            return await Task.Run(() =>
                _context.UsuariosDto
                .FromSqlRaw("EXEC PA_OBTENER_USUARIO @Id_Usuario", idParam)
                .AsNoTracking()
                .AsEnumerable()
                .FirstOrDefault()
            );
        }

        public async Task<UsuarioCredencial?> ObtenerCredencialAsync(string correoElectronico)
        {
            var correoParam = new SqlParameter("@Correo_Electronico", correoElectronico);

            var resultado = await _context.Set<UsuarioCredencial>()
                .FromSqlRaw("EXEC PA_INICIAR_SESION @Correo_Electronico = @Correo_Electronico", correoParam)
                .AsNoTracking()
                .ToListAsync();

            return resultado.FirstOrDefault();
        }

        public async Task<int> ActualizarClaveAsync(int idUsuario, string claveHash)
        {
            return await _context.Database.ExecuteSqlRawAsync(
                "EXEC PA_ACTUALIZAR_CLAVE @Id_Usuario = @Id_Usuario, @Clave = @Clave",
                new SqlParameter("@Id_Usuario", idUsuario),
                new SqlParameter("@Clave", claveHash)
            );
        }

        public async Task<int> RegistrarUsuarioAsync(Usuario usuario)
        {
            string claveHash = ClaveHasher.Hashear(usuario.Clave ?? "");

            return await _context.Database.ExecuteSqlRawAsync(
                "EXEC PA_REGISTRAR_USUARIO @Codigo = @Codigo, @Nombre_Completo = @Nombre_Completo, @Correo_Electronico = @Correo_Electronico, " +
                "@Clave = @Clave, @Id_Rol = @Id_Rol, @Estado = @Estado, @Id_Usuario_Creacion = @Id_Usuario_Creacion",
                new SqlParameter("@Codigo", usuario.Codigo ?? (object)DBNull.Value),
                new SqlParameter("@Nombre_Completo", usuario.Nombre_Completo ?? (object)DBNull.Value),
                new SqlParameter("@Correo_Electronico", usuario.Correo_Electronico ?? (object)DBNull.Value),
                new SqlParameter("@Clave", claveHash),
                new SqlParameter("@Id_Rol", usuario.Id_Rol ?? (object)DBNull.Value),
                new SqlParameter("@Estado", usuario.Estado ?? (object)DBNull.Value),
                ParametroUsuario("@Id_Usuario_Creacion")
            );
        }

        public async Task<int> EditarUsuarioAsync(Usuario usuario)
        {
            var usuarioActual = await _context.Usuario
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Id_Usuario == usuario.Id_Usuario && !u.Eliminado);

            if (usuarioActual == null)
                throw new Exception("Usuario no encontrado");

            string claveHash = !string.IsNullOrEmpty(usuario.Clave)
                ? ClaveHasher.Hashear(usuario.Clave)
                : usuarioActual.Clave ?? "";

            return await _context.Database.ExecuteSqlRawAsync(
                "EXEC PA_EDITAR_USUARIO @Id_Usuario = @Id_Usuario, @Nombre_Completo = @Nombre_Completo, @Correo_Electronico = @Correo_Electronico, " +
                "@Clave = @Clave, @Id_Rol = @Id_Rol, @Estado = @Estado, @Id_Usuario_Modificacion = @Id_Usuario_Modificacion",
                new SqlParameter("@Id_Usuario", usuario.Id_Usuario),
                new SqlParameter("@Nombre_Completo", usuario.Nombre_Completo ?? (object)DBNull.Value),
                new SqlParameter("@Correo_Electronico", usuario.Correo_Electronico ?? (object)DBNull.Value),
                new SqlParameter("@Clave", claveHash),
                new SqlParameter("@Id_Rol", usuario.Id_Rol ?? (object)DBNull.Value),
                new SqlParameter("@Estado", usuario.Estado ?? (object)DBNull.Value),
                ParametroUsuario("@Id_Usuario_Modificacion")
            );
        }

        public async Task<int> EliminarUsuarioAsync(int idUsuario)
        {
            return await _context.Database.ExecuteSqlRawAsync(
                "EXEC PA_ELIMINAR_USUARIO @Id_Usuario = @Id_Usuario, @Id_Usuario_Modificacion = @Id_Usuario_Modificacion",
                new SqlParameter("@Id_Usuario", idUsuario),
                ParametroUsuario("@Id_Usuario_Modificacion")
            );
        }
    }
}
