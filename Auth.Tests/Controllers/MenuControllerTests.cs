using APIRestSistemaVentas.Controllers;
using Domain.Model;
using Infrastructure.Repository.InterfacesServices;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System.Security.Claims;
using Utilities.Shared;

namespace Auth.Tests.Controllers
{
    [TestClass]
    public class MenuControllerTests
    {
        private Mock<IMenuService> _servicio = null!;

        [TestInitialize]
        public void Inicializar()
        {
            _servicio = new Mock<IMenuService>();
            _servicio.Setup(s => s.ObtenerMenusAsync(It.IsAny<int>()))
                     .ReturnsAsync(new ApiResponse<List<Menu>> { IsSuccess = true, Data = new List<Menu>() });
        }

        private MenuController Controlador(int idUsuario, string rol) => new(_servicio.Object)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = new ClaimsPrincipal(new ClaimsIdentity(new[]
                    {
                        new Claim(ClaimTypes.NameIdentifier, idUsuario.ToString()),
                        new Claim(ClaimTypes.Role, rol)
                    }, "Test"))
                }
            }
        };

        [TestMethod]
        public async Task GetMenus_SusPropiosMenus_Ok()
        {
            var resultado = await Controlador(5, "Empleado").GetMenus(5);

            Assert.IsInstanceOfType<OkObjectResult>(resultado.Result);
        }

        [TestMethod]
        public async Task GetMenus_EmpleadoPideMenusDeOtroUsuario_Prohibido()
        {
            var resultado = await Controlador(5, "Empleado").GetMenus(1);

            Assert.IsInstanceOfType<ForbidResult>(resultado.Result);
            _servicio.Verify(s => s.ObtenerMenusAsync(It.IsAny<int>()), Times.Never);
        }

        [TestMethod]
        public async Task GetMenus_AdministradorPideMenusDeOtroUsuario_Ok()
        {
            var resultado = await Controlador(1, "Administrador").GetMenus(5);

            Assert.IsInstanceOfType<OkObjectResult>(resultado.Result);
        }
    }
}
