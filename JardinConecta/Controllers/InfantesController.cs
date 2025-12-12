using JardinConecta.Http.Requests;
using JardinConecta.Infrastructure.Repository;
using JardinConecta.Models.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace JardinConecta.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class InfantesController : AbstractController
    {
        public InfantesController(ServiceContext context) : base(context)
        {
        }

        [HttpPost]
        [Authorize(Roles = $"{TipoUsuario.ROL_ADMIN_JARDIN},{TipoUsuario.ROL_ADMIN_SISTEMA}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Create(AltaInfanteRequest request)
        {
            var now = DateTime.UtcNow;
            var idUsuarioLogueado = User.GetIdUsuario();

            Guid idJardin;
            try
            {
                idJardin = await SelectIdJardin(request);
            }
            catch (InvalidOperationException)
            {
                return BadRequest();
            }

            bool existeInfante = await _context.Set<Infante>().Where(i => i.IdJardin == idJardin && i.Documento == request.Documento).AnyAsync();

            if (existeInfante) return Forbid();

            var infante = new Infante()
            {
                Id = Guid.NewGuid(),
                IdJardin = idJardin,
                Nombre = request.Nombre,
                Apellido = request.Apellido,
                Documento = request.Documento,
                FechaNacimiento = request.FechaNacimiento
            };

            await _context.AddAsync(infante);
            await _context.SaveChangesAsync();

            return Ok();
        }



    }
}
