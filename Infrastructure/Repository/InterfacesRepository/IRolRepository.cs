using Domain.Model;

namespace Infrastructure.Repository.InterfacesRepository
{
    public interface IRolRepository
    {
        Task<List<Rol>> ListarRolesAsync();
    }
}
