using JardinConecta.Http.Requests;
using JardinConecta.Http.Responses;
using JardinConecta.Infrastructure.Repository;
using JardinConecta.Models.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace JardinConecta.Controllers
{
    [Authorize]
    [ApiController]
    [Route("[controller]")]
    public class SalasController : AbstractController
    {
        public SalasController(ServiceContext context) : base(context)
        {
        }

        [HttpPost]
        [Authorize(Roles = $"{TipoUsuario.ROL_ADMIN_JARDIN},{TipoUsuario.ROL_ADMIN_SISTEMA}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Create(AltaSalaRequest request)
        {
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

            var sala = new Sala()
            {
                Id = Guid.NewGuid(),
                IdJardin = idJardin,
                Nombre = request.Nombre
            };

            await _context.AddAsync(sala);
            await _context.SaveChangesAsync();

            return Ok();
        }

        [HttpGet]
        [ProducesResponseType(typeof(SalasResponse), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAllAsync([FromQuery] Guid? idJardin)
        {
            var result = await _context.Set<Sala>().Where(x => (idJardin == null || x.IdJardin == idJardin))
                .Select(x => new SalasResponse(x.Id, x.Nombre))
                .ToListAsync();

            return Ok(result);
        }

        [HttpGet("{id}")]
        [ProducesResponseType(typeof(SalaDetalleResponse), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetByIdAsync(Guid id)
        {
            var result = await _context.Set<Sala>()
                .Include(x => x.UsuariosSalasRoles)
                    .ThenInclude(x => x.Usuario)
                    .ThenInclude(x => x.Persona)
                .Where(x => x.Id == id)
                .Select(x => new SalaDetalleResponse(
                    x.Id,
                    x.Nombre,
                    x.UsuariosSalasRoles.Select(x => new SalaDetalleResponse_UsuariosMiembros(x.IdUsuario, x.Usuario.Persona!.Nombre, x.Usuario.Persona.Apellido)).ToList()))
                .FirstOrDefaultAsync();

            if (result == null)
                return NotFound();

            return Ok(result);
        }


    }
}
