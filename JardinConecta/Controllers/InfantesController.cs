using JardinConecta.Core.Entities;
using JardinConecta.Core.Interfaces;
using JardinConecta.Core.Services.Dtos;
using JardinConecta.Models.Http.Requests;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace JardinConecta.Controllers
{
    [Authorize]
    [ApiController]
    [Route("[controller]")]
    public class InfantesController : ControllerBase
    {
        private readonly IAdminJardinService _adminJardinService;
        private readonly IInfantesService _infantesService;

        public InfantesController(
            IAdminJardinService adminJardinService,
            IInfantesService infantesService
        )
        {
            _adminJardinService = adminJardinService;
            _infantesService = infantesService;
        }

        [HttpPost]
        [Authorize(Roles = $"{TipoUsuario.ROL_ADMIN_JARDIN},{TipoUsuario.ROL_ADMIN_SISTEMA}")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Create(AltaInfanteRequest request)
        {
            Guid idJardin = await _adminJardinService.SelectIdJardin(HttpContext, request.IdJardin);

            await _infantesService.AltaDeInfante(idJardin, request.Nombre, request.Apellido, request.Documento, request.FechaNacimiento);

            return Created();
        }

        [HttpGet]
        [Authorize(Roles = $"{TipoUsuario.ROL_ADMIN_JARDIN},{TipoUsuario.ROL_ADMIN_SISTEMA}")]
        [ProducesResponseType(typeof(List<InfanteResult>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAll([FromQuery] Guid? idJardin, [FromQuery] Guid? idSala)
        {
            int tipoUsuario = User.GetTipoUsuario();
            Guid _idJardin = tipoUsuario == (int)TipoUsuarioId.AdminJardin
                ? User.GetIdJardin()
                : idJardin ?? Guid.Empty;

            var result = await _infantesService.ObtenerInfantes(_idJardin, idSala);

            return Ok(result);
        }

        [HttpGet("{infanteId}")]
        [Authorize(Roles = $"{TipoUsuario.ROL_ADMIN_JARDIN},{TipoUsuario.ROL_ADMIN_SISTEMA}")]
        [ProducesResponseType(typeof(InfanteDetalleResult), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetById(Guid infanteId)
        {
            var infante = await _infantesService.ObtenerInfante(infanteId);

            return Ok(infante);
        }

        [HttpPut("{infanteId}")]
        [Authorize(Roles = $"{TipoUsuario.ROL_ADMIN_JARDIN},{TipoUsuario.ROL_ADMIN_SISTEMA}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Update(Guid infanteId, EditarInfanteRequest request)
        {
            await _infantesService.ActualizarInfante(infanteId, request.Nombre, request.Apellido, request.Documento, request.FechaNacimiento);

            return NoContent();
        }

        [HttpDelete("{infanteId}")]
        [Authorize(Roles = $"{TipoUsuario.ROL_ADMIN_JARDIN},{TipoUsuario.ROL_ADMIN_SISTEMA}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<IActionResult> Delete(Guid infanteId)
        {
            await _infantesService.EliminarInfante(infanteId);

            return NoContent();
        }

        [HttpDelete("{infanteId}/Salas/{salaId}")]
        [Authorize(Roles = $"{TipoUsuario.ROL_ADMIN_JARDIN},{TipoUsuario.ROL_ADMIN_SISTEMA}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DesasignarSala(Guid infanteId, Guid salaId)
        {
            await _infantesService.DesasignarSala(infanteId, salaId);

            return NoContent();
        }

        [HttpPost("{infanteId}/Salas/{salaId}")]
        [Authorize(Roles = $"{TipoUsuario.ROL_ADMIN_JARDIN},{TipoUsuario.ROL_ADMIN_SISTEMA}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<IActionResult> AsignarSala(Guid infanteId, Guid salaId)
        {
            await _infantesService.AsignarSala(infanteId, salaId);

            return NoContent();
        }

        [HttpDelete("{infanteId}/Tutelas/{usuarioId}")]
        [Authorize(Roles = $"{TipoUsuario.ROL_ADMIN_JARDIN},{TipoUsuario.ROL_ADMIN_SISTEMA}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteTutela(Guid infanteId, Guid usuarioId)
        {
            await _infantesService.DesasignarTutela(infanteId, usuarioId);

            return NoContent();
        }

        [HttpPost("Importar")]
        [Authorize(Roles = $"{TipoUsuario.ROL_ADMIN_JARDIN},{TipoUsuario.ROL_ADMIN_SISTEMA}")]
        [Consumes("multipart/form-data")]
        [ProducesResponseType(typeof(ImportarInfantesResult), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Importar([FromQuery] Guid? idJardin, [FromForm] ImportarInfantesRequest request)
        {
            Guid _idJardin = await _adminJardinService.SelectIdJardin(HttpContext, idJardin);

            var result = await _infantesService.ImportarInfantes(_idJardin, request.Archivo);

            return Ok(result);
        }
    }
}
