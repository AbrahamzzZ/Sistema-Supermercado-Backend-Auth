using Utilities.Security;

namespace Infrastructure.Security
{
    public static class ClaveHasher
    {
        private const int WorkFactor = 11;

        public static string Hashear(string clave) => BCrypt.Net.BCrypt.HashPassword(clave, WorkFactor);
        public static bool Verificar(string clave, string? hashGuardado, out bool requiereRehash)
        {
            requiereRehash = false;
            if (string.IsNullOrEmpty(hashGuardado))
                return false;

            if (EsHashLegado(hashGuardado))
            {
                var valido = string.Equals(hashGuardado, Encriptacion.EncriptarSHA256(clave), StringComparison.OrdinalIgnoreCase);
                requiereRehash = valido;
                return valido;
            }

            try
            {
                return BCrypt.Net.BCrypt.Verify(clave, hashGuardado);
            }
            catch (BCrypt.Net.SaltParseException)
            {
                return false;
            }
        }

        private static bool EsHashLegado(string hash) => hash.Length == 64 && hash.All(Uri.IsHexDigit);
    }
}
