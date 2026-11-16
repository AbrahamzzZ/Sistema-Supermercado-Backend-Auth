using Domain.Model;
using Infrastructure.Repository.InterfacesServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Utilities.Shared;

namespace APIRestSistemaVentas.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class MenuController : ControllerBase
    {
        private readonly IMenuService _menuService;

        public MenuController(IMenuService menuService)
        {
            _menuService = menuService;
        }

        // GET: api/menu/1
        [HttpGet("{idUsuario}")]
        public async Task<ActionResult<ApiResponse<List<Menu>>>> GetMenus(int idUsuario)
        {
            var esPropio = User.FindFirstValue(ClaimTypes.NameIdentifier) == idUsuario.ToString();
            if (!esPropio && !User.IsInRole("Administrador"))
                return Forbid();

            var respuesta = await _menuService.ObtenerMenusAsync(idUsuario);
            return Ok(respuesta);
        }
    }
}
