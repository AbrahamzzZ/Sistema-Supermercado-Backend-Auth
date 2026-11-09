using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using System.Security.Cryptography;

namespace Infrastructure.Helpers
{
    /// <summary>
    /// Administra la clave RSA con la que Auth firma los JWT (RS256).
    /// - La clave PRIVADA solo existe en este microservicio.
    /// - La clave PÚBLICA se publica en /.well-known/jwks.json para que
    ///   Gateway, Admin, Inventory y Order validen los tokens sin poder crearlos.
    ///
    /// Origen de la clave privada (en este orden):
    ///   1. Jwt:PrivateKeyPath -> ruta a un archivo .pem (Docker: /run/secrets/jwt_private_key)
    ///   2. Jwt:PrivateKey     -> el contenido PEM directamente (útil con dotnet user-secrets)
    ///   3. Ninguno            -> se genera una clave temporal (solo para desarrollo:
    ///                            los tokens dejan de ser válidos al reiniciar Auth)
    /// </summary>
    public sealed class RsaKeyProvider : IDisposable
    {
        private readonly RSA _rsa;

        public RsaSecurityKey PrivateKey { get; }
        public RsaSecurityKey PublicKey { get; }
        public SigningCredentials SigningCredentials { get; }
        public string KeyId { get; }

        public RsaKeyProvider(IConfiguration configuration, ILogger<RsaKeyProvider>? logger = null)
        {
            _rsa = RSA.Create();

            var path = configuration["Jwt:PrivateKeyPath"];
            var pem = configuration["Jwt:PrivateKey"];

            if (!string.IsNullOrWhiteSpace(path))
            {
                if (!File.Exists(path))
                    throw new FileNotFoundException($"No se encontró la clave privada JWT en '{path}'.", path);
                _rsa.ImportFromPem(File.ReadAllText(path));
            }
            else if (!string.IsNullOrWhiteSpace(pem))
            {
                _rsa.ImportFromPem(pem);
            }
            else
            {
                _rsa.KeySize = 2048;
                logger?.LogWarning("No se configuró Jwt:PrivateKeyPath ni Jwt:PrivateKey. " + "Se generó una clave RSA temporal: los tokens serán inválidos al reiniciar Auth.");
            }

            if (_rsa.KeySize < 2048)
                throw new InvalidOperationException("La clave RSA para JWT debe ser de al menos 2048 bits.");

            var publicRsa = RSA.Create();
            publicRsa.ImportParameters(_rsa.ExportParameters(includePrivateParameters: false));
            var tempKey = new RsaSecurityKey(publicRsa);
            KeyId = Base64UrlEncoder.Encode(tempKey.ComputeJwkThumbprint());

            PublicKey = new RsaSecurityKey(publicRsa) { KeyId = KeyId };
            PrivateKey = new RsaSecurityKey(_rsa) { KeyId = KeyId };
            SigningCredentials = new SigningCredentials(PrivateKey, SecurityAlgorithms.RsaSha256);
        }

        /// <summary>Documento JWKS con la clave pública (nunca incluye la parte privada).</summary>
        public object GetJwks()
        {
            var jwk = JsonWebKeyConverter.ConvertFromRSASecurityKey(PublicKey);
            return new
            {
                keys = new[]
                {
                    new
                    {
                        kty = jwk.Kty,
                        use = "sig",
                        alg = SecurityAlgorithms.RsaSha256,
                        kid = KeyId,
                        n = jwk.N,
                        e = jwk.E
                    }
                }
            };
        }

        public void Dispose() => _rsa.Dispose();
    }
}
