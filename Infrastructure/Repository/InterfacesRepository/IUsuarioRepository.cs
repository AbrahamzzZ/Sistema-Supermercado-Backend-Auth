using Domain.Model;
using Domain.Model.Dto;
using Utilities.Shared;

namespace Infrastructure.Repository.InterfacesRepository
{
    public interface IUsuarioRepository
    {
        Task<List<UsuarioRol>> ListarUsuariosAsync();
        Task<Paginacion<UsuarioRol>> ListarUsuariosPaginacionAsync(int pageNumber, int pageSize);
        Task<UsuarioRol?> ObtenerUsuarioAsync(int idUsuario);
        Task<UsuarioCredencial?> ObtenerCredencialAsync(string correoElectronico);
        Task<int> ActualizarClaveAsync(int idUsuario, string claveHash);
        Task<int> RegistrarUsuarioAsync(Usuario usuario);
        Task<int> EditarUsuarioAsync(Usuario usuario);
        Task<int> EliminarUsuarioAsync(int idUsuario);
    }
}
