using JardinConecta.Core.Entities;
using JardinConecta.Core.Interfaces;
using JardinConecta.Core.Services.Dtos;
using JardinConecta.Models.Http.Requests;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace JardinConecta.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class AdminController : ControllerBase
    {
        private readonly IAdminJardinService _adminJardinService;
        private readonly ICodigosDeInvitacionService _codigosDeInvitacionService;

        public AdminController(
            IAdminJardinService adminJardinService,
            ICodigosDeInvitacionService codigosDeInvitacionService
        )
        {
            _adminJardinService = adminJardinService;
            _codigosDeInvitacionService = codigosDeInvitacionService;
        }

        [HttpPost("Invitaciones")]
        [Authorize(Roles = $"{TipoUsuario.ROL_ADMIN_JARDIN},{TipoUsuario.ROL_ADMIN_SISTEMA}")]
        [ProducesResponseType(typeof(CodigoInvitacionResult), StatusCodes.Status201Created)]
        public async Task<IActionResult> GenerarInvitacion(AltaCodigoInvitacionRequest request)
        {
            Guid idJardin = await _adminJardinService.SelectIdJardin(HttpContext, request.IdJardin);

            var invitacion = await _codigosDeInvitacionService.GenerarCodigoInvitacionSala(idJardin, request.IdSala, request.FechaExpiracion, request.TipoInvitacion, request.IdInfante);

            return CreatedAtAction(nameof(GenerarInvitacion), invitacion);
        }

        [HttpGet("Invitaciones")]
        [Authorize(Roles = $"{TipoUsuario.ROL_ADMIN_JARDIN},{TipoUsuario.ROL_ADMIN_SISTEMA}")]
        [ProducesResponseType(typeof(List<CodigoInvitacionItemResult>), StatusCodes.Status200OK)]
        public async Task<IActionResult> ListarInvitaciones([FromQuery] Guid idSala, [FromQuery] Guid? idJardin)
        {
            Guid _idJardin = await _adminJardinService.SelectIdJardin(HttpContext, idJardin);

            var codigosDeInvitacion = await _codigosDeInvitacionService.ListarCodigosInvitacion(_idJardin, idSala);

            return Ok(codigosDeInvitacion);
        }
    }
}
