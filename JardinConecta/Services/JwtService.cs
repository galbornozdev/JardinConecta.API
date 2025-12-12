using JardinConecta.Common;
using JardinConecta.Configurations;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace JardinConecta.Services
{
    public class JwtService : IJwtService
    {
        private readonly JwtOptions _jwtOptions;

        public JwtService(IOptions<JwtOptions> jwtOptions)
        {
            _jwtOptions = jwtOptions.Value;
        }

        public (string, DateTime) GenerateToken(Guid userId, string email, string role, Guid? IdJardin = null)
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtOptions.Key));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new List<Claim>() {
                new Claim(JwtRegisteredClaimNames.Sub, userId.ToString()),
                new Claim(JwtRegisteredClaimNames.Email, email),
                new Claim(ClaimTypes.Role, role),
                new Claim(Constants.CUSTOM_CLAIMS__ID_USUARIO, userId.ToString())
            };

            if (IdJardin is not null)
                claims.Add(new Claim(Constants.CUSTOM_CLAIMS__ID_JARDIN, IdJardin.ToString()!));

            var expires = DateTime.Now.AddHours(24);

            var token = new JwtSecurityToken(
                issuer: _jwtOptions.Issuer,
                audience: null,
                claims: claims,
                expires: expires,
                signingCredentials: creds
            );

            return (new JwtSecurityTokenHandler().WriteToken(token), expires);
        }
    }
}
