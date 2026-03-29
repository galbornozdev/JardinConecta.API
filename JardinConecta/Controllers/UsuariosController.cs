using JardinConecta.Models.Http.Requests;
using JardinConecta.Services.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace JardinConecta.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class UsuariosController : ControllerBase
    {
        private readonly IUsuariosService _usuariosService;

        public UsuariosController(
            IUsuariosService usuariosService
        )
        {
            _usuariosService = usuariosService;
        }

        [HttpPatch("Me")]
        [Authorize]
        public async Task<IActionResult> ActualizarPerfil([FromBody] ActualizarPerfilRequest request)
        {
            var idUsuario = User.GetIdUsuario();

            await _usuariosService.ActualizarInformacionPersonal(idUsuario, request.Nombre, request.Apellido, request.Documento);

            return Ok();
        }

        [HttpPost("Me/Photo")]
        [Authorize]
        public async Task<IActionResult> SubirFoto(IFormFile photo)
        {
            if (photo == null || photo.Length == 0)
                return BadRequest();

            var idUsuario = User.GetIdUsuario();
            var urlFoto = await _usuariosService.ActualizarFotoPerfil(idUsuario, photo);

            return Ok(urlFoto);
        }

        [HttpPost("RegistrarDispositivo")]
        [Authorize]
        public async Task<IActionResult> RegistrarDispositivo([FromBody] RegistrarDispositivoRequest request)
        {
            var idUsuario = User.GetIdUsuario();

            await _usuariosService.ActualizarDeviceToken(idUsuario, request.DeviceToken);

            return Ok();
        }

        [HttpPost("")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        public async Task<IActionResult> Create([FromBody]AltaUsuarioRequest request)
        {
            await _usuariosService.AltaDeUsuario(request.Email, request.Password);
            return Created();
        }

    }
}
