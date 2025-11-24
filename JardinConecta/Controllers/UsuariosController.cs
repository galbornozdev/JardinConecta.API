using JardinConecta.Models.Entities;
using JardinConecta.Repository;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace JardinConecta.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class UsuariosController : ControllerBase
    {
        private readonly ServiceContext _context;

        public UsuariosController(ServiceContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IEnumerable<Usuario>> Get()
        {
            var usuarios = await _context.Set<Usuario>().ToListAsync();
            return usuarios;
        }
    }
}
