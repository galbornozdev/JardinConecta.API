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
    public class ComunicadosController : ControllerBase
    {
        private readonly IComunicadosService _comunicadosService;
        private readonly ISalasService _salasService;

        public ComunicadosController(
            IComunicadosService comunicadosService,
            ISalasService salasService
        )
        {
            _comunicadosService = comunicadosService;
            _salasService = salasService;
        }

        [HttpGet]
        [Authorize]
        [ProducesResponseType(typeof(PagedResult<ComunicadoItemResult>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetPaginated(
            [FromQuery] Guid? idSala,
            [FromQuery] int page,
            [FromQuery] int? pageSize,
            [FromQuery] int? estado,
            [FromQuery] DateTime? fechaDesde,
            [FromQuery] DateTime? fechaHasta)
        {
            var idTipoUsuario = User.GetTipoUsuario();
            var idUsuario = User.GetIdUsuario();
            Guid? idJardin = (idTipoUsuario == (int)TipoUsuarioId.AdminJardin) ? User.GetIdJardin() : null;

            var filtros = new ComunicadosFilterDto(page, pageSize, estado, fechaDesde, fechaHasta);
            var result = await _comunicadosService.ObtenerComunicadosPaginados(idSala, idTipoUsuario, idUsuario, idJardin, filtros);

            return Ok(result);
        }

        [HttpGet("{id}")]
        [Authorize]
        [ProducesResponseType(typeof(ComunicadoDetalleResult), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetById(Guid id)
        {
            var idUsuario = User.GetIdUsuario();
            var idTipoUsuario = User.GetTipoUsuario();

            var result = await _comunicadosService.ObtenerComunicado(id, idUsuario, idTipoUsuario);

            return Ok(result);
        }

        [HttpPost]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> Create([FromForm] AltaComunicadoRequest request)
        {
            var idUsuario = User.GetIdUsuario();
            var idTipoUsuario = User.GetTipoUsuario();

            if (idTipoUsuario == (int)TipoUsuarioId.AdminJardin)
            {
                if (!await _salasService.CheckSalaPerteneceJardin(User.GetIdJardin(), request.IdSala)) return Forbid();
            }
            else if (!await _salasService.CheckUsuarioPerteneceASala(request.IdSala, idUsuario, (int)RolId.Educador)) return Forbid();

            if (request.Estado == (int)EstadoComunicado.Programado)
            {
                if (request.FechaPrograma == null || request.FechaPrograma <= DateTime.UtcNow)
                    return BadRequest(new { message = "FechaPrograma debe ser una fecha futura para comunicados programados" });
            }

            var nuevoComunicado = new ComunicadoDto(
                request.Titulo,
                request.Contenido,
                request.ContenidoTextoPlano,
                request.FechaPrograma,
                request.Estado);

            await _comunicadosService.CrearNuevoComunicado(request.IdSala, idUsuario, nuevoComunicado, request.Archivos);

            return Ok();
        }

        [HttpPut("{id}")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Update(Guid id, [FromForm] EditarComunicadoRequest request)
        {
            var idUsuario = User.GetIdUsuario();

            var comunicadoData = new ComunicadoDto(
                request.Titulo,
                request.Contenido,
                request.ContenidoTextoPlano,
                request.FechaPrograma,
                request.Estado);

            await _comunicadosService.ModificarComunicado(id, idUsuario, comunicadoData, request.Archivos, request.ArchivosEliminar);

            return Ok();
        }

        [HttpPost("{id}/Publicar")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Publicar(Guid id)
        {
            var idUsuario = User.GetIdUsuario();

            await _comunicadosService.PublicarComunicado(id, idUsuario);

            return Ok();
        }

        [HttpGet("{id}/Views")]
        [Authorize]
        [ProducesResponseType(typeof(IEnumerable<ComunicadoViewDetalleResult>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetViews(Guid id)
        {
            var result = await _comunicadosService.ObtenerViews(id);

            return Ok(result);
        }

        [HttpDelete("{id}")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Delete(Guid id)
        {
            var idUsuario = User.GetIdUsuario();

            await _comunicadosService.EliminarComunicado(id, idUsuario);

            return NoContent();
        }
    }
}
