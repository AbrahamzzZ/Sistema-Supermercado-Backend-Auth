using Domain.Model;
using Domain.Model.Dto;
using Microsoft.Extensions.Configuration;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace Infrastructure.Helpers
{
    public class Token
    {
        private readonly IConfiguration _configuration;
        private readonly RsaKeyProvider _keyProvider;

        public Token(IConfiguration configuration, RsaKeyProvider keyProvider)
        {
            _configuration = configuration;
            _keyProvider = keyProvider;
        }

        public string GenerarToken(UsuarioRol usuario, List<Menu> permisos)
        {
            var jwtSettings = _configuration.GetSection("Jwt");

            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, usuario.Id_Usuario.ToString()),
                new(ClaimTypes.Name, usuario.Nombre_Completo ?? ""),
                new(ClaimTypes.Email, usuario.Correo_Electronico ?? ""),
                new(ClaimTypes.Role, usuario.Nombre_Rol ?? "Usuario"),
                new("codigo", usuario.Codigo ?? ""),
                new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            foreach (var menu in permisos)
            {
                claims.Add(new Claim("permiso", menu.CodigoMenu ?? ""));
            }

            var ahora = DateTime.UtcNow;

            var token = new JwtSecurityToken(
                issuer: jwtSettings["Issuer"],
                audience: jwtSettings["Audience"],
                claims: claims,
                notBefore: ahora,
                expires: ahora.AddMinutes(jwtSettings.GetValue<int>("DurationInMinutes")),
                signingCredentials: _keyProvider.SigningCredentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
