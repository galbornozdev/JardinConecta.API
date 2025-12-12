using JardinConecta.Http.Requests;
using JardinConecta.Models.Entities;
using JardinConecta.Repository;
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
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> Create(AltaSalaRequest request)
        {
            var now = DateTime.UtcNow;
            var idUsuarioLogueado = User.GetIdUsuario();
            var idRol = User.GetIdRol();

            Guid idJardin = await SelectIdJardin(request);

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
