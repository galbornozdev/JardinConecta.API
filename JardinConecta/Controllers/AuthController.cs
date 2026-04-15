using JardinConecta.Core.Entities;
using JardinConecta.Core.Interfaces;
using JardinConecta.Core.Services.Dtos;
using JardinConecta.Models.Http.Requests;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

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
            var tipoUsuario = (TipoUsuarioId)User.GetTipoUsuario();
            Guid? idJardin = tipoUsuario == TipoUsuarioId.AdminJardin
                ? User.GetIdJardin()
                : null;


            var response = await _usuariosService.ObtenerUsuarioLogueado(IdUsuarioLogueado, tipoUsuario, idJardin);

            return Ok(response);
        }

        [HttpPost("Login")]
        [EnableRateLimiting("login")]
        [ProducesResponseType(typeof(LoginResult), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
        public async Task<IActionResult> Login(LoginRequest request)
        {
            var result = await _authService.Login(request.Email, request.Password);

            return Ok(result);
        }
    }
}
