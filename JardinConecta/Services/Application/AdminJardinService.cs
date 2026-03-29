using JardinConecta.Common;
using JardinConecta.Infrastructure.Repository;
using JardinConecta.Models.Entities;
using JardinConecta.Services.Application.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace JardinConecta.Services.Application
{
    public class AdminJardinService : IAdminJardinService
    {
        private readonly ServiceContext _context;

        public AdminJardinService(
            ServiceContext context
        )
        {
            _context = context;
        }

        public async Task<Guid> SelectIdJardin(HttpContext httpContext, Guid? idJardin)
        {
            if (httpContext.User.GetTipoUsuario() == (int)TipoUsuarioId.AdminJardin)
            {
                return httpContext.User.GetIdJardin();
            }
            else
            {
                if (idJardin == null) throw new ArgumentException("Debe proporcionar un identificador de jardin.");

                var existeJardin = await _context.Set<Jardin>().Where(j => j.Id == idJardin).AnyAsync();

                if (!existeJardin) throw new ArgumentException("El identificador de jardin es incorrecto.");

                return (Guid)idJardin;
            }
        }
    }
}
