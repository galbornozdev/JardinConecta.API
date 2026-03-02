using JardinConecta.Infrastructure.Repository;
using JardinConecta.Models.Entities;
using JardinConecta.Models.Http.Requests;
using JardinConecta.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Cryptography;

namespace JardinConecta.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class UsuariosController : AbstractController
    {
        public UsuariosController(ServiceContext context) : base(context)
        {
        }

        [HttpPost("Usuario")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> Create([FromBody]AltaUsuarioRequest request)
        {
            var now = DateTime.UtcNow;

            var nuevoUsuario = new Usuario()
            {
                Id = Guid.NewGuid(),
                Email = request.Email,
                PasswordHash = PasswordHasher.Hash(request.Password),
                Persona = new Persona()
                {
                    Nombre = request.Nombre,
                    Apellido = request.Apellido,
                    Documento = request.Documento,
                },
                Telefono = new Telefono()
                {
                    CaracteristicaPais = request.CaracteristicaPais,
                    CodigoArea = request.CodigoArea,
                    Numero = request.Numero,
                },
                IdTipoUsuario = (int)TipoUsuarioId.Usuario,
                CreatedAt = now,
                UpdatedAt = now
            };

            var tokenVerifiacionEmail = new TokenVerificacionEmail()
            {
                Id = Guid.NewGuid(),
                IdUsuario = nuevoUsuario.Id,
                Token = GenerateRandomString(),
                FechaExpiracion = now.AddHours(1),
            };

            await _context.AddAsync(nuevoUsuario);
            await _context.AddAsync(tokenVerifiacionEmail);

            await _context.SaveChangesAsync();
            return Ok();
        }

        private static string GenerateRandomString(int length = 20)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            var bytes = RandomNumberGenerator.GetBytes(length);

            return new string(bytes.Select(b => chars[b % chars.Length]).ToArray());
        }
    }
}
