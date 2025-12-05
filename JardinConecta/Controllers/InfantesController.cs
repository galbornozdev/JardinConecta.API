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
        [Authorize(Roles = $"{Rol.ROL_ADMIN_JARDIN},{Rol.ROL_ADMIN_SISTEMA}")]
        public async Task<IActionResult> Create(AltaInfanteRequest request)
        {

            throw new NotImplementedException();
            if (User.FindFirstValue(ClaimTypes.Role) == Rol.ROL_ADMIN_JARDIN) {
                var userId = User.FindFirst("uid")?.Value;
                var idJardin = (await _context.Set<Usuario>().FindAsync(userId))?.IdJardin;
                if (idJardin == null) return Forbid();

                request.IdJardin = idJardin;
            }

            if (request.IdJardin == null) return BadRequest();

            //Infante infante = new() {
            //    Id = Guid.NewGuid(),
            //    IdJardin = request.IdJardin!,

            //};

            //_context.Set<Infante>().

            return Ok();
        }




    }
}
