using JardinConecta.Http;
using JardinConecta.Models.Entities;
using JardinConecta.Repository;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace JardinConecta.Controllers
{
    public abstract class AbstractController : ControllerBase
    {
        protected ServiceContext _context;

        public AbstractController(ServiceContext context)
        {
            _context = context;
        }

        protected async Task<Guid> SelectIdJardin(IHasIdJardin request)
        {
            Guid idJardin;
            int idRol = User.GetIdRol();
            if (idRol == (int)TipoUsuarioId.AdminJardin)
            {
                idJardin = User.GetIdJardin();
            }
            else
            {
                if (request.IdJardin == null) throw new InvalidOperationException();
                idJardin = (Guid)request.IdJardin;

                var existeJardin = await _context.Set<Jardin>().Where(j => j.Id == request.IdJardin).AnyAsync();

                if (!existeJardin) throw new InvalidOperationException();
            }

            return idJardin;
        }
    }
}
