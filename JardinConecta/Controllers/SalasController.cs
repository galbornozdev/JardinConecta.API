using JardinConecta.Models.Entities;
using JardinConecta.Models.Http.Requests;
using JardinConecta.Models.Http.Responses;
using JardinConecta.Services.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace JardinConecta.Controllers
{
    [Authorize]
    [ApiController]
    [Route("[controller]")]
    public class SalasController : ControllerBase
    {
        private readonly IAdminJardinService _adminJardinService;
        private readonly ISalasService _salasService;

        public SalasController(
            IAdminJardinService adminJardinService,
            ISalasService salasService
        )
        {
            _adminJardinService = adminJardinService;
            _salasService = salasService;
        }

        [HttpPost]
        [Authorize(Roles = $"{TipoUsuario.ROL_ADMIN_JARDIN},{TipoUsuario.ROL_ADMIN_SISTEMA}")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Create(AltaSalaRequest request)
        {
            Guid idJardin = await _adminJardinService.SelectIdJardin(HttpContext, request.IdJardin);

            await _salasService.CrearSala(idJardin, request.Nombre);

            return Created();
        }

        [HttpGet]
        [Authorize]
        [ProducesResponseType(typeof(SalasResponse), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAllAsync([FromQuery] Guid? idJardin)
        {
            int tipoUsuario = User.GetTipoUsuario();
            Guid? jardinFilter = tipoUsuario == (int)TipoUsuarioId.AdminJardin
                ? User.GetIdJardin()
                : idJardin;

            return Ok(await _salasService.ObtenerSalas(jardinFilter));
        }

        [HttpGet("{salaId}/Miembros")]
        [Authorize(Roles = $"{TipoUsuario.ROL_ADMIN_JARDIN},{TipoUsuario.ROL_ADMIN_SISTEMA}")]
        [ProducesResponseType(typeof(ICollection<SalaMiembroResponse>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetMiembros(Guid salaId)
        {
            return Ok(await _salasService.ObtenerMiembros(salaId));
        }

        [HttpPost("{salaId}/Educadores")]
        [Authorize(Roles = $"{TipoUsuario.ROL_ADMIN_JARDIN},{TipoUsuario.ROL_ADMIN_SISTEMA}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<IActionResult> AgregarEducador(Guid salaId, AgregarEducadorRequest request)
        {
            await _salasService.AsociarEducadorMedianteEmail(salaId, request.Email);
            return NoContent();
        }

        [HttpDelete("{salaId}/Miembros/{usuarioId}")]
        [Authorize(Roles = $"{TipoUsuario.ROL_ADMIN_JARDIN},{TipoUsuario.ROL_ADMIN_SISTEMA}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteMiembro(Guid salaId, Guid usuarioId)
        {
            await _salasService.DesasociarUsuario(salaId, usuarioId);

            return NoContent();
        }

        [HttpDelete("Desvincular/{salaId}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Desvincular(Guid salaId)
        {
            var idUsuario = User.GetIdUsuario();

            await _salasService.DesasociarUsuario(salaId, idUsuario);

            return NoContent();
        }

        [HttpGet("{id}")]
        [ProducesResponseType(typeof(SalaDetalleResponse), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetByIdAsync(Guid id)
        {
            return Ok(await _salasService.ObtenerSala(id));
        }
    }
}
