using Domain.Model;
using Domain.Model.Dto;
using FluentValidation;
using Infrastructure.Repository;
using Infrastructure.Repository.InterfacesServices;
using Infrastructure.Security;
using Microsoft.Extensions.Logging;
using Utilities.Shared;

namespace Infrastructure.Services
{
    public class UsuarioService : IUsuarioService
    {
        private readonly UsuarioRepository _usuarioRepository;
        private readonly IValidator<Usuario> _validator;
        private readonly ILogger<UsuarioService> _logger;

        public UsuarioService(UsuarioRepository usuarioRepository, IValidator<Usuario> validator, ILogger<UsuarioService> logger)
        {
            _usuarioRepository = usuarioRepository;
            _validator = validator;
            _logger = logger;
        }

        public async Task<ApiResponse<List<UsuarioRol>>> ListarUsuariosAsync()
        {
            var listaUsuarios = await _usuarioRepository.ListarUsuariosAsync();

            if (listaUsuarios == null || listaUsuarios.Count == 0)
                return new ApiResponse<List<UsuarioRol>> { IsSuccess = false, Message = Mensajes.MESSAGE_QUERY_EMPTY, Data = listaUsuarios };

            return new ApiResponse<List<UsuarioRol>> { IsSuccess = true, Message = Mensajes.MESSAGE_QUERY, Data = listaUsuarios };
        }

        public async Task<ApiResponse<Paginacion<UsuarioRol>>> ListarUsuariosPaginacionAsync(int pageNumber, int pageSize)
        {
            var pagedResult = await _usuarioRepository.ListarUsuariosPaginacionAsync(pageNumber, pageSize);

            if (pagedResult.Items == null || pagedResult.Items.Count == 0)
            {
                return new ApiResponse<Paginacion<UsuarioRol>> { IsSuccess = false, Message = Mensajes.MESSAGE_QUERY_EMPTY, Data = pagedResult };
            }

            return new ApiResponse<Paginacion<UsuarioRol>> { IsSuccess = true, Message = Mensajes.MESSAGE_QUERY, Data = pagedResult };
        }

        public async Task<ApiResponse<UsuarioRol>> ObtenerUsuarioAsync(int idUsuario)
        {
            var usuario = await _usuarioRepository.ObtenerUsuarioAsync(idUsuario);

            if (usuario == null)
            {
                return new ApiResponse<UsuarioRol> { IsSuccess = false, Message = Mensajes.MESSAGE_QUERY_NOT_FOUND };
            }

            return new ApiResponse<UsuarioRol> { IsSuccess = true, Message = Mensajes.MESSAGE_QUERY, Data = usuario };
        }

        public async Task<ApiResponse<UsuarioRol>> IniciarSesionAsync(Login login)
        {
            if (login == null || string.IsNullOrWhiteSpace(login.Correo_Electronico) || string.IsNullOrWhiteSpace(login.Clave))
            {
                _logger.LogWarning("Login rechazado: correo o clave vacíos");
                return new ApiResponse<UsuarioRol> { IsSuccess = false, Message = "Correo y clave son obligatorios." };
            }

            var credencial = await _usuarioRepository.ObtenerCredencialAsync(login.Correo_Electronico);

            if (credencial == null || !ClaveHasher.Verificar(login.Clave, credencial.Clave, out bool requiereRehash))
            {
                _logger.LogWarning("Login fallido para {CorreoElectronico}: credenciales inválidas", login.Correo_Electronico);
                return new ApiResponse<UsuarioRol> { IsSuccess = false, Message = "Credenciales inválidas." };
            }

            if (credencial.Estado == false)
            {
                _logger.LogWarning("Login rechazado para el usuario {IdUsuario}: usuario inactivo", credencial.Id_Usuario);
                return new ApiResponse<UsuarioRol>
                { IsSuccess = false, Message = "Usuario inactivo. Contacte con el administrador." };
            }

            if (requiereRehash)
            {
                await _usuarioRepository.ActualizarClaveAsync(credencial.Id_Usuario, ClaveHasher.Hashear(login.Clave));
                _logger.LogInformation("Clave del usuario {IdUsuario} migrada de SHA-256 a BCrypt", credencial.Id_Usuario);
            }

            _logger.LogInformation("Login exitoso del usuario {IdUsuario}", credencial.Id_Usuario);
            return new ApiResponse<UsuarioRol> { IsSuccess = true, Message = "Inicio de sesión exitoso.", Data = credencial.ToUsuarioRol() };
        }

        public async Task<ApiResponse<object>> RegistrarUsuarioAsync(Usuario usuario)
        {
            var validationResult = await _validator.ValidateAsync(usuario);

            if (!validationResult.IsValid)
            {
                var errores = string.Join(" | ", validationResult.Errors.Select(e => e.ErrorMessage));
                _logger.LogWarning("Registro de usuario rechazado por validación: {Errores}", errores);
                return new ApiResponse<object> { IsSuccess = false, Message = errores };
            }

            var usuarios = await _usuarioRepository.ListarUsuariosAsync();
            if (usuarios.Any(c => c.Codigo == usuario.Codigo))
            {
                _logger.LogWarning("Registro de usuario rechazado: el código {Codigo} ya existe", usuario.Codigo);
                return new ApiResponse<object> { IsSuccess = false, Message = "El código ya existe" };
            }

            if (usuarios.Any(c => c.Nombre_Completo == usuario.Nombre_Completo))
            {
                _logger.LogWarning("Registro de usuario rechazado: el nombre {NombreCompleto} ya existe", usuario.Nombre_Completo);
                return new ApiResponse<object> { IsSuccess = false, Message = "Ese nombre ya existe" };
            }

            var result = await _usuarioRepository.RegistrarUsuarioAsync(usuario);
            if (result > 0)
            {
                _logger.LogInformation("Usuario {Codigo} registrado", usuario.Codigo);
                return new ApiResponse<object> { IsSuccess = true, Message = Mensajes.MESSAGE_REGISTER };
            }

            _logger.LogWarning("No se pudo registrar el usuario {Codigo}", usuario.Codigo);
            return new ApiResponse<object> { IsSuccess = false, Message = Mensajes.MESSAGE_REGISTER_FAILLED };
        }

        public async Task<ApiResponse<object>> EditarUsuarioAsync(Usuario usuario)
        {
            if (usuario == null)
                return new ApiResponse<object> { IsSuccess = false, Message = Mensajes.MESSAGE_NULL };

            var validationResult = await _validator.ValidateAsync(usuario);
            if (!validationResult.IsValid)
            {
                var errores = string.Join(" | ", validationResult.Errors.Select(e => e.ErrorMessage));
                _logger.LogWarning("Edición del usuario {IdUsuario} rechazada por validación: {Errores}", usuario.Id_Usuario, errores);
                return new ApiResponse<object> { IsSuccess = false, Message = errores };
            }

            var usuarioActual = await _usuarioRepository.ObtenerUsuarioAsync(usuario.Id_Usuario);
            if (usuarioActual == null)
            {
                _logger.LogWarning("Edición rechazada: el usuario {IdUsuario} no existe", usuario.Id_Usuario);
                return new ApiResponse<object> { IsSuccess = false, Message = Mensajes.MESSAGE_QUERY_NOT_FOUND };
            }

            if (usuarioActual.Nombre_Rol == "Administrador" && usuarioActual.Estado == true)
            {
                var totalAdmins = (await _usuarioRepository.ListarUsuariosAsync())
                    .Count(u => u.Nombre_Rol == "Administrador" && u.Estado == true);

                if (totalAdmins <= 1 &&
                    (usuario.Estado == false || usuario.Id_Rol != usuarioActual.Id_Rol))
                {
                    _logger.LogWarning("Edición rechazada: el usuario {IdUsuario} es el único administrador activo", usuario.Id_Usuario);
                    return new ApiResponse<object> { IsSuccess = false, Message = "No se puede modificar el rol al único administrador activo." };
                }
            }

            var usuarios = await _usuarioRepository.ListarUsuariosAsync();
            if (usuarios.Any(c => c.Nombre_Completo == usuario.Nombre_Completo && c.Id_Usuario != usuario.Id_Usuario))
            {
                _logger.LogWarning("Edición del usuario {IdUsuario} rechazada: el nombre {NombreCompleto} ya existe", usuario.Id_Usuario, usuario.Nombre_Completo);
                return new ApiResponse<object> { IsSuccess = false, Message = "El nombre ya existe." };
            }

            var result = await _usuarioRepository.EditarUsuarioAsync(usuario);
            if (result > 0)
            {
                _logger.LogInformation("Usuario {IdUsuario} editado", usuario.Id_Usuario);
                return new ApiResponse<object> { IsSuccess = true, Message = Mensajes.MESSAGE_UPDATE };
            }

            _logger.LogWarning("No se pudo editar el usuario {IdUsuario}", usuario.Id_Usuario);
            return new ApiResponse<object> { IsSuccess = false, Message = Mensajes.MESSAGE_UPDATE_FAILLED };
        }

        public async Task<ApiResponse<int>> EliminarUsuarioAsync(int id)
        {
            var existe = await _usuarioRepository.ObtenerUsuarioAsync(id);
            if (existe == null)
            {
                _logger.LogWarning("Eliminación rechazada: el usuario {IdUsuario} no existe", id);
                return new ApiResponse<int> { IsSuccess = false, Message = Mensajes.MESSAGE_QUERY_NOT_FOUND };
            }

            if (existe.Nombre_Rol == "Administrador" && existe.Estado == true)
            {
                var totalAdmins = (await _usuarioRepository.ListarUsuariosAsync())
                    .Count(u => u.Nombre_Rol == "Administrador" && u.Estado == true);

                if (totalAdmins <= 1)
                {
                    _logger.LogWarning("Eliminación rechazada: el usuario {IdUsuario} es el único administrador activo", id);
                    return new ApiResponse<int> { IsSuccess = false, Message = "No se puede eliminar al único administrador activo." };
                }
            }

            // Borrado lógico: el usuario queda con ELIMINADO = 1 (sus compras/ventas siguen apuntando a él)
            var result = await _usuarioRepository.EliminarUsuarioAsync(id);

            if (result > 0)
            {
                _logger.LogInformation("Usuario {IdUsuario} eliminado (borrado lógico)", id);
                return new ApiResponse<int> { IsSuccess = true, Message = Mensajes.MESSAGE_DELETE };
            }

            _logger.LogWarning("No se pudo eliminar el usuario {IdUsuario}", id);
            return new ApiResponse<int> { IsSuccess = false, Message = Mensajes.MESSAGE_DELETE_FAILLED };
        }
    }
}
