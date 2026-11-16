using Domain.Model;
using Utilities.Shared;

namespace Infrastructure.Repository.InterfacesServices
{
    public interface IMenuService
    {
        Task<ApiResponse<List<Menu>>> ObtenerMenusAsync(int idUsuario);
    }
}
