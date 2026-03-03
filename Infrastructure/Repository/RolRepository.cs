using Domain.Context;
using Domain.Model;
using Infrastructure.Repository.InterfacesRepository;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repository
{
    public class RolRepository : IRolRepository
    {
        private readonly AuthSistemaSupermercadoContext _context;

        public RolRepository(AuthSistemaSupermercadoContext context)
        {
            _context = context;
        }

        public async Task<List<Rol>> ListarRolesAsync()
        {
            return await _context.Rol
                .FromSqlRaw("EXEC PA_LISTA_Rol")
                .ToListAsync();
        }
    }
}
