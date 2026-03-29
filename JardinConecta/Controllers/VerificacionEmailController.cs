using JardinConecta.Models.ViewModels;
using JardinConecta.Services.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace JardinConecta.Controllers
{
    [ApiController]
    [Route("")]
    public class VerificacionEmailController : Controller
    {
        private readonly IUsuariosService _usuariosService;

        public VerificacionEmailController(
            IUsuariosService usuariosService
        )
        {
            _usuariosService = usuariosService;
        }

        [HttpGet("verificar-email")]
        public async Task<IActionResult> Verificar(string token)
        {
            var ok = await _usuariosService.VerificarEmail(token);

            if(!ok)
            {
                return View("EmailVerificado", new VerificacionEmailResultViewModel() { Success = false });
            }

            return View("EmailVerificado", new VerificacionEmailResultViewModel() { Success = true });
        }
    }
}