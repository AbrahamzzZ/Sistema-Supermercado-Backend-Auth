using Domain.Model;
using Domain.Model.Dto;
using Infrastructure.Helpers;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace Auth.Tests.Security
{
    [TestClass]
    public class TokenTests
    {
        private static IConfiguration Configuracion() => new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Jwt:Issuer"] = "SupermercadoAPI",
                ["Jwt:Audience"] = "SupermercadoClient",
                ["Jwt:DurationInMinutes"] = "60"
                // Sin Jwt:PrivateKeyPath: RsaKeyProvider genera una clave temporal (no se usa la real).
            })
            .Build();

        [TestMethod]
        public void GenerarToken_FirmadoRs256_LoValidaLaClavePublicaDelJwks()
        {
            var configuracion = Configuracion();
            using var claves = new RsaKeyProvider(configuracion);
            var usuario = new UsuarioRol { Id_Usuario = 4, Codigo = "USU004", Nombre_Completo = "Ana", Nombre_Rol = "Empleado" };
            var menus = new List<Menu> { new() { UrlMenu = "/venta" }, new() { UrlMenu = "/producto" } };

            var jwt = new Token(configuracion, claves).GenerarToken(usuario, menus);

            // Se valida igual que lo hacen el Gateway y los demás microservicios: solo con la clave pública.
            var principal = new JwtSecurityTokenHandler().ValidateToken(jwt, new TokenValidationParameters
            {
                ValidIssuer = "SupermercadoAPI",
                ValidAudience = "SupermercadoClient",
                IssuerSigningKey = claves.PublicKey,
                ValidAlgorithms = new[] { SecurityAlgorithms.RsaSha256 }
            }, out var tokenValidado);

            Assert.AreEqual(SecurityAlgorithms.RsaSha256, ((JwtSecurityToken)tokenValidado).Header.Alg);
            Assert.AreEqual("4", principal.FindFirstValue(ClaimTypes.NameIdentifier));
            Assert.IsTrue(principal.IsInRole("Empleado"));
            CollectionAssert.AreEquivalent(new[] { "/venta", "/producto" }, principal.FindAll("permiso").Select(c => c.Value).ToArray());
        }

        [TestMethod]
        public void GenerarToken_FirmadoPorOtraClave_EsRechazado()
        {
            var configuracion = Configuracion();
            using var claves = new RsaKeyProvider(configuracion);
            using var otraClave = new RsaKeyProvider(configuracion);
            var jwt = new Token(configuracion, otraClave).GenerarToken(new UsuarioRol { Id_Usuario = 1 }, new List<Menu>());

            Assert.ThrowsException<SecurityTokenSignatureKeyNotFoundException>(() =>
                new JwtSecurityTokenHandler().ValidateToken(jwt, new TokenValidationParameters
                {
                    ValidIssuer = "SupermercadoAPI",
                    ValidAudience = "SupermercadoClient",
                    IssuerSigningKey = claves.PublicKey
                }, out _));
        }
    }
}
