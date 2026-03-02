using JardinConecta.Models.ViewModels;
using JardinConecta.Services;
using Microsoft.AspNetCore.Mvc;

namespace JardinConecta.Controllers
{
    [ApiController]
    [Route("")]
    public class VerificacionEmailController : Controller
    {
        [HttpGet("verificar-email")]
        public async Task<IActionResult> Verificar(string token)
        {
            return View("EmailVerificado", new VerificacionEmailViewModel() { Success = true });
        }
    }
}
