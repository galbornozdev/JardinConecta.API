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

    }
}
