using Domain.Context;
using Domain.Model;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Utilities.Shared;

namespace Infrastructure.Services
{
    public class MenuService
    {
        private readonly AuthSistemaSupermercadoContext _context;
        private readonly ILogger<MenuService> _logger;

        public MenuService(AuthSistemaSupermercadoContext context, ILogger<MenuService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<ApiResponse<List<Menu>>> ObtenerMenusAsync(int idUsuario)
        {
            var idParam = new SqlParameter("@Id_Usuario", idUsuario);
            var menus = await _context.Menu
                .FromSqlRaw("EXEC PA_OBTENER_MENU @Id_Usuario", idParam)
                .AsNoTracking()
                .ToListAsync();

            if (menus.Count == 0)
            {
                _logger.LogWarning("No hay menús disponibles para el usuario {IdUsuarioConsultado}", idUsuario);
                return new ApiResponse<List<Menu>> { IsSuccess = false, Message = Mensajes.MESSAGE_QUERY_EMPTY, Data = menus };
            }

            _logger.LogInformation("Se obtuvieron {Cantidad} menús para el usuario {IdUsuarioConsultado}", menus.Count, idUsuario);
            return new ApiResponse<List<Menu>> { IsSuccess = true, Message = Mensajes.MESSAGE_QUERY, Data = menus };
        }
    }
}
