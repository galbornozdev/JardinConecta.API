using JardinConecta.Core.Interfaces;
using JardinConecta.Core.Services.Dtos;
using JardinConecta.Models.Http.Requests;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace JardinConecta.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class InvitacionesController : ControllerBase
    {
        private readonly ICodigosDeInvitacionService _codigosDeInvitacionService;

        public InvitacionesController(
            ICodigosDeInvitacionService codigosDeInvitacionService
        )
        {
            _codigosDeInvitacionService = codigosDeInvitacionService;
        }

        [HttpGet("Verificar")]
        [Authorize]
        [ProducesResponseType(typeof(VerificarInvitacionResult), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Verificar([FromQuery] string codigo)
        {
            return Ok(await _codigosDeInvitacionService.VerificarCodigo(codigo));
        }

        [HttpPost("Canjear")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Canjear(CanjearInvitacionRequest request)
        {
            var idUsuario = User.GetIdUsuario();

            await _codigosDeInvitacionService.CanjearCodigo(idUsuario, request.Codigo, request.DocumentoSufijo, request.IdTipoTutela);

            return NoContent();
        }
    }
}
