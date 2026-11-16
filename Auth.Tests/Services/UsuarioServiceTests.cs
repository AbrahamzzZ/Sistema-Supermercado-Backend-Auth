using Domain.Model;
using Domain.Model.Dto;
using Infrastructure.Repository.InterfacesRepository;
using Infrastructure.Security;
using Infrastructure.Services;
using Infrastructure.Services.Validators;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Utilities.Security;

namespace Auth.Tests.Services
{
    [TestClass]
    public class UsuarioServiceTests
    {
        private const string Correo = "admin@correo.com";
        private const string Clave = "Admin123*Segura";

        private Mock<IUsuarioRepository> _repositorio = null!;
        private UsuarioService _servicio = null!;

        [TestInitialize]
        public void Inicializar()
        {
            _repositorio = new Mock<IUsuarioRepository>();
            _servicio = new UsuarioService(_repositorio.Object, new UsuarioValidator(), NullLogger<UsuarioService>.Instance);
        }

        private static UsuarioCredencial Credencial(string hash, bool activo = true) => new()
        {
            Id_Usuario = 1, Codigo = "USU001", Nombre_Completo = "Admin", Correo_Electronico = Correo,
            Id_Rol = 1, Nombre_Rol = "Administrador", Estado = activo, Clave = hash
        };

        private static bool EsBCryptDe(string hash) => ClaveHasher.Verificar(Clave, hash, out _);

        private static UsuarioRol Admin(int id) => new() { Id_Usuario = id, Id_Rol = 1, Nombre_Rol = "Administrador", Estado = true };

        // ---------- Login ----------

        [TestMethod]
        public async Task IniciarSesion_CamposVacios_RechazaSinConsultarLaBase()
        {
            var resultado = await _servicio.IniciarSesionAsync(new Login { Correo_Electronico = Correo, Clave = " " });

            Assert.IsFalse(resultado.IsSuccess);
            _repositorio.Verify(r => r.ObtenerCredencialAsync(It.IsAny<string>()), Times.Never);
        }

        [TestMethod]
        public async Task IniciarSesion_ClaveIncorrecta_NoDevuelveDatosDelUsuario()
        {
            _repositorio.Setup(r => r.ObtenerCredencialAsync(Correo)).ReturnsAsync(Credencial(ClaveHasher.Hashear(Clave)));

            var resultado = await _servicio.IniciarSesionAsync(new Login { Correo_Electronico = Correo, Clave = "incorrecta" });

            Assert.IsFalse(resultado.IsSuccess);
            Assert.AreEqual("Credenciales inválidas.", resultado.Message);
            Assert.IsNull(resultado.Data);
        }

        [TestMethod]
        public async Task IniciarSesion_CorreoInexistente_MismoMensajeQueClaveIncorrecta()
        {
            // No se debe revelar si el correo existe o no.
            var resultado = await _servicio.IniciarSesionAsync(new Login { Correo_Electronico = "no@existe.com", Clave = Clave });

            Assert.IsFalse(resultado.IsSuccess);
            Assert.AreEqual("Credenciales inválidas.", resultado.Message);
        }

        [TestMethod]
        public async Task IniciarSesion_UsuarioInactivo_Rechaza()
        {
            _repositorio.Setup(r => r.ObtenerCredencialAsync(Correo)).ReturnsAsync(Credencial(ClaveHasher.Hashear(Clave), activo: false));

            var resultado = await _servicio.IniciarSesionAsync(new Login { Correo_Electronico = Correo, Clave = Clave });

            Assert.IsFalse(resultado.IsSuccess);
            StringAssert.Contains(resultado.Message, "inactivo");
        }

        [TestMethod]
        public async Task IniciarSesion_Correcto_DevuelveElUsuarioSinLaClave()
        {
            _repositorio.Setup(r => r.ObtenerCredencialAsync(Correo)).ReturnsAsync(Credencial(ClaveHasher.Hashear(Clave)));

            var resultado = await _servicio.IniciarSesionAsync(new Login { Correo_Electronico = Correo, Clave = Clave });

            Assert.IsTrue(resultado.IsSuccess);
            Assert.AreEqual(1, resultado.Data!.Id_Usuario);
            Assert.AreEqual("Administrador", resultado.Data.Nombre_Rol);
            _repositorio.Verify(r => r.ActualizarClaveAsync(It.IsAny<int>(), It.IsAny<string>()), Times.Never);
        }

        [TestMethod]
        public async Task IniciarSesion_ConHashLegadoSha256_MigraLaClaveABCrypt()
        {
            _repositorio.Setup(r => r.ObtenerCredencialAsync(Correo)).ReturnsAsync(Credencial(Encriptacion.EncriptarSHA256(Clave)));

            var resultado = await _servicio.IniciarSesionAsync(new Login { Correo_Electronico = Correo, Clave = Clave });

            Assert.IsTrue(resultado.IsSuccess);
            _repositorio.Verify(r => r.ActualizarClaveAsync(1, It.Is<string>(h => h.StartsWith("$2") && EsBCryptDe(h))), Times.Once);
        }

        // ---------- Único administrador activo ----------

        [TestMethod]
        public async Task EliminarUsuario_UnicoAdministradorActivo_Rechaza()
        {
            _repositorio.Setup(r => r.ObtenerUsuarioAsync(1)).ReturnsAsync(Admin(1));
            _repositorio.Setup(r => r.ListarUsuariosAsync()).ReturnsAsync(new List<UsuarioRol> { Admin(1) });

            var resultado = await _servicio.EliminarUsuarioAsync(1);

            Assert.IsFalse(resultado.IsSuccess);
            _repositorio.Verify(r => r.EliminarUsuarioAsync(It.IsAny<int>()), Times.Never);
        }

        [TestMethod]
        public async Task EliminarUsuario_AdministradorConOtroAdminActivo_Permite()
        {
            _repositorio.Setup(r => r.ObtenerUsuarioAsync(1)).ReturnsAsync(Admin(1));
            _repositorio.Setup(r => r.ListarUsuariosAsync()).ReturnsAsync(new List<UsuarioRol> { Admin(1), Admin(2) });
            _repositorio.Setup(r => r.EliminarUsuarioAsync(1)).ReturnsAsync(1);

            var resultado = await _servicio.EliminarUsuarioAsync(1);

            Assert.IsTrue(resultado.IsSuccess);
        }

        [TestMethod]
        public async Task EditarUsuario_DesactivarAlUnicoAdministrador_Rechaza()
        {
            _repositorio.Setup(r => r.ObtenerUsuarioAsync(1)).ReturnsAsync(Admin(1));
            _repositorio.Setup(r => r.ListarUsuariosAsync()).ReturnsAsync(new List<UsuarioRol> { Admin(1) });
            var edicion = new Usuario
            {
                Id_Usuario = 1, Nombre_Completo = "Admin", Correo_Electronico = Correo, Id_Rol = 1, Estado = false
            };

            var resultado = await _servicio.EditarUsuarioAsync(edicion);

            Assert.IsFalse(resultado.IsSuccess);
            _repositorio.Verify(r => r.EditarUsuarioAsync(It.IsAny<Usuario>()), Times.Never);
        }
    }
}
