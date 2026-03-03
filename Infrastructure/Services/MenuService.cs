using Domain.Context;
using Domain.Model;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Services
{
    public class MenuService 
    {
        private readonly AuthSistemaSupermercadoContext _context;

        public MenuService(AuthSistemaSupermercadoContext context)
        {
            _context = context;
        }

        public async Task<List<Menu>> ObtenerMenusAsync(int idUsuario)
        {
            var idParam = new SqlParameter("@Id_Usuario", idUsuario);
            return await _context.Menu
                .FromSqlRaw("EXEC PA_OBTENER_MENU @Id_Usuario", idParam)
                .AsNoTracking()
                .ToListAsync();
        }
    }
}
