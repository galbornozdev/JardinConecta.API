using JardinConecta.Models.Http.Requests;
using JardinConecta.Services.Application.Dtos;
using JardinConecta.Services.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace JardinConecta.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly IUsuariosService _usuariosService;

        public AuthController(
            IAuthService authService,
            IUsuariosService usuariosService
        )
        {
            _authService = authService;
            _usuariosService = usuariosService;
        }

        [HttpGet("Me")]
        [Authorize]
        [ProducesResponseType(typeof(UsuarioLogueadoResult), StatusCodes.Status200OK)]
        public async Task<IActionResult> Me()
        {
            var IdUsuarioLogueado = User.GetIdUsuario();

            var response = await _usuariosService.ObtenerUsuario(IdUsuarioLogueado);

            return Ok(response);
        }

        [HttpPost("Login")]
        [ProducesResponseType(typeof(LoginResult), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> Login(LoginRequest request)
        {
            var result = await _authService.Login(request.Email, request.Password);

            return Ok(result);
        }
    }
}
