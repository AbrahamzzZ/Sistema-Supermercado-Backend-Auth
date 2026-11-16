using Infrastructure.Security;
using Utilities.Security;

namespace Auth.Tests.Security
{
    [TestClass]
    public class ClaveHasherTests
    {
        [TestMethod]
        public void Hashear_GeneraBCryptQueSeVerificaConLaMismaClave()
        {
            var hash = ClaveHasher.Hashear("Admin123*Segura");

            Assert.IsTrue(hash.StartsWith("$2"), "Debe ser un hash BCrypt");
            Assert.IsTrue(ClaveHasher.Verificar("Admin123*Segura", hash, out var requiereRehash));
            Assert.IsFalse(requiereRehash);
        }

        [TestMethod]
        public void Verificar_ClaveIncorrecta_DevuelveFalse()
        {
            var hash = ClaveHasher.Hashear("Admin123*Segura");

            Assert.IsFalse(ClaveHasher.Verificar("otra-clave", hash, out _));
        }

        [TestMethod]
        public void Verificar_HashLegadoSha256Correcto_ValidaYPideMigrarABCrypt()
        {
            var hashLegado = Encriptacion.EncriptarSHA256("ClaveVieja1*");

            Assert.IsTrue(ClaveHasher.Verificar("ClaveVieja1*", hashLegado, out var requiereRehash));
            Assert.IsTrue(requiereRehash);
        }

        [TestMethod]
        public void Verificar_HashLegadoSha256Incorrecto_NoPideMigrar()
        {
            var hashLegado = Encriptacion.EncriptarSHA256("ClaveVieja1*");

            Assert.IsFalse(ClaveHasher.Verificar("otra", hashLegado, out var requiereRehash));
            Assert.IsFalse(requiereRehash);
        }

        [DataTestMethod]
        [DataRow(null)]
        [DataRow("")]
        [DataRow("texto-que-no-es-un-hash")]
        public void Verificar_HashVacioOCorrupto_DevuelveFalseSinExcepcion(string? hash)
        {
            Assert.IsFalse(ClaveHasher.Verificar("cualquiera", hash, out _));
        }
    }
}
