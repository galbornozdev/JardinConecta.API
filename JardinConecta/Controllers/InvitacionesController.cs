using JardinConecta.Infrastructure.Repository;
using JardinConecta.Models.Entities;
using JardinConecta.Models.Http.Requests;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace JardinConecta.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class InvitacionesController : AbstractController
    {
        public InvitacionesController(ServiceContext context) : base(context)
        {
        }

        [HttpPost("Canjear")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<IActionResult> Canjear(CanjearInvitacionRequest request)
        {
            var now = DateTime.UtcNow;

            var invitacion = await _context.Set<CodigoInvitacion>()
                .Include(c => c.Infante)
                .Where(c => c.Codigo == request.Codigo && c.FechaExpiracion > now)
                .FirstOrDefaultAsync();

            if (invitacion is null) return NotFound();

            var documento = invitacion.Infante.Documento?.Trim();
            if (documento is null || documento.Length < 3) return Forbid();

            var sufijo = documento[^3..];
            if (!sufijo.Equals(request.DocumentoSufijo.Trim(), StringComparison.OrdinalIgnoreCase))
                return Forbid();

            var idUsuario = User.GetIdUsuario();

            var yaMiembro = await _context.Set<UsuarioSalaRol>()
                .AnyAsync(u => u.IdUsuario == idUsuario && u.IdSala == invitacion.IdSala);

            if (yaMiembro) return Conflict();

            await _context.AddAsync(new UsuarioSalaRol
            {
                IdUsuario = idUsuario,
                IdSala = invitacion.IdSala,
                IdRol = (int)RolId.Familia,
                CreatedAt = now
            });

            var tutelaExiste = await _context.Set<Tutela>()
                .AnyAsync(t => t.IdUsuario == idUsuario && t.IdInfante == invitacion.IdInfante);

            if (!tutelaExiste)
            {
                await _context.AddAsync(new Tutela
                {
                    IdUsuario = idUsuario,
                    IdInfante = invitacion.IdInfante,
                    IdTipoTutela = request.IdTipoTutela,
                    CreatedAt = now
                });
            }

            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
