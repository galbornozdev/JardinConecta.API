using JardinConecta.Infrastructure.Repository;
using JardinConecta.Models.Entities;
using JardinConecta.Models.ViewModels;
using JardinConecta.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace JardinConecta.Controllers
{
    [ApiController]
    [Route("")]
    public class VerificacionEmailController : Controller
    {
        private readonly ServiceContext _context;

        public VerificacionEmailController(ServiceContext serviceContext)
        {
            _context = serviceContext;
        }

        [HttpGet("verificar-email")]
        public async Task<IActionResult> Verificar(string token)
        {
            var now = DateTime.UtcNow;

            var tokenVerificacionEmail = await _context.Set<TokenVerificacionEmail>()
                .Include(x => x.Usuario)
                .Where(t => t.Token == token && t.FechaUtilizacion == null && t.FechaExpiracion > now)
                .FirstOrDefaultAsync();

            if(tokenVerificacionEmail == null)
            {
                return View("EmailVerificado", new VerificacionEmailViewModel() { Success = false });
            }

            tokenVerificacionEmail.FechaUtilizacion = now;
            tokenVerificacionEmail.Usuario.FechaVerificacionEmail = now;

            await _context.SaveChangesAsync();

            return View("EmailVerificado", new VerificacionEmailViewModel() { Success = true });
        }
    }
}