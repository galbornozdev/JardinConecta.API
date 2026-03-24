using JardinConecta.Infrastructure.Repository;
using JardinConecta.Models.Entities;
using JardinConecta.Models.Http.Requests;
using JardinConecta.Models.Http.Responses;
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

        [HttpGet("Verificar")]
        [Authorize]
        [ProducesResponseType(typeof(VerificarInvitacionResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Verificar([FromQuery] string codigo)
        {
            var now = DateTime.UtcNow;

            var invitacion = await _context.Set<CodigoInvitacion>()
                .Include(c => c.Sala).ThenInclude(s => s.Jardin)
                .Where(c => c.Codigo == codigo && c.FechaExpiracion > now)
                .FirstOrDefaultAsync();

            if (invitacion is null) return NotFound();

            var tipo = invitacion.TipoInvitacion == (int)TipoInvitacion.Educador
                ? nameof(TipoInvitacion.Educador)
                : nameof(TipoInvitacion.Familia);

            return Ok(new VerificarInvitacionResponse
            {
                TipoInvitacion = tipo,
                NombreSala = invitacion.Sala.Nombre,
                NombreJardin = invitacion.Sala.Jardin.Nombre
            });
        }

        [HttpPost("Canjear")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Canjear(CanjearInvitacionRequest request)
        {
            var now = DateTime.UtcNow;

            var invitacion = await _context.Set<CodigoInvitacion>()
                .Include(c => c.Infante)
                .Where(c => c.Codigo == request.Codigo && c.FechaExpiracion > now)
                .FirstOrDefaultAsync();

            if (invitacion is null) return NotFound();

            var idUsuario = User.GetIdUsuario();

            if (invitacion.TipoInvitacion == (int)TipoInvitacion.Educador)
            {
                var yaMiembro = await _context.Set<UsuarioSalaRol>()
                    .AnyAsync(u => u.IdUsuario == idUsuario && u.IdSala == invitacion.IdSala);

                if (!yaMiembro)
                {
                    await _context.AddAsync(new UsuarioSalaRol
                    {
                        IdUsuario = idUsuario,
                        IdSala = invitacion.IdSala,
                        IdRol = (int)RolId.Educador,
                        CreatedAt = now
                    });
                }
            }
            else
            {
                if (string.IsNullOrWhiteSpace(request.DocumentoSufijo) || request.IdTipoTutela is null)
                    return BadRequest();

                var documento = invitacion.Infante?.Documento?.Trim();
                if (documento is null || documento.Length < 3) return Forbid();

                var sufijo = documento[^3..];
                if (!sufijo.Equals(request.DocumentoSufijo.Trim(), StringComparison.OrdinalIgnoreCase))
                    return Forbid();

                var yaMiembro = await _context.Set<UsuarioSalaRol>()
                    .AnyAsync(u => u.IdUsuario == idUsuario && u.IdSala == invitacion.IdSala);

                if (!yaMiembro)
                {
                    await _context.AddAsync(new UsuarioSalaRol
                    {
                        IdUsuario = idUsuario,
                        IdSala = invitacion.IdSala,
                        IdRol = (int)RolId.Familia,
                        CreatedAt = now
                    });
                }

                var tutela = await _context.Set<Tutela>()
                    .FirstOrDefaultAsync(t => t.IdUsuario == idUsuario && t.IdInfante == invitacion.IdInfante);

                if (tutela is null)
                {
                    await _context.AddAsync(new Tutela
                    {
                        IdUsuario = idUsuario,
                        IdInfante = invitacion.IdInfante!.Value,
                        IdTipoTutela = request.IdTipoTutela.Value,
                        CreatedAt = now
                    });
                }
                else
                {
                    tutela.IdTipoTutela = request.IdTipoTutela.Value;
                }
            }

            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
