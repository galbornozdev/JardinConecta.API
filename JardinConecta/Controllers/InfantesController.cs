using JardinConecta.Http.Requests;
using JardinConecta.Models.Entities;
using JardinConecta.Repository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace JardinConecta.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class InfantesController : ControllerBase
    {
        private ServiceContext _context;

        public InfantesController(ServiceContext context)
        {
            _context = context;
        }

        [HttpPost]
        [Authorize(Roles = $"{TipoUsuario.ROL_ADMIN_JARDIN},{TipoUsuario.ROL_ADMIN_SISTEMA}")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> Create(AltaInfanteRequest request)
        {
            var now = DateTime.UtcNow;
            var idUsuarioLogueado = User.GetIdUsuario();
            var idRol = User.GetIdRol();

            Guid idJardin;
            if (idRol == (int)TipoUsuarioId.AdminJardin)
            {
                idJardin = User.GetIdJardin();
            }
            else
            {
                if (request.IdJardin == null) return BadRequest();
                idJardin = (Guid)request.IdJardin;

                var existeJardin = await _context.Set<Jardin>().Where(j => j.Id == request.IdJardin).AnyAsync();

                if (!existeJardin) return BadRequest();
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
