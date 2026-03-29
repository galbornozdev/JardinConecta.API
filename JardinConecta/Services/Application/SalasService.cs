using JardinConecta.Infrastructure.Repository;
using JardinConecta.Models.Entities;
using JardinConecta.Models.ViewModels.EmailTemplates;
using JardinConecta.Services.Application.Dtos;
using JardinConecta.Services.Application.Interfaces;
using JardinConecta.Services.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace JardinConecta.Services.Application
{
    public class SalasService : ISalasService
    {
        private readonly ServiceContext _context;
        private readonly IEmailService _emailService;

        public SalasService(
            ServiceContext context,
            IEmailService emailService
        )
        {
            _context = context;
            _emailService = emailService;
        }

        public async Task CrearSala(Guid idJardin, string nombre)
        {
            var sala = new Sala()
            {
                Id = Guid.NewGuid(),
                IdJardin = idJardin,
                Nombre = nombre
            };

            await _context.AddAsync(sala);
            await _context.SaveChangesAsync();
        }

        public async Task<List<SalaResult>> ObtenerSalas(Guid? idJardin)
        {
            var result = await _context.Set<Sala>()
                .Where(x => idJardin == null || x.IdJardin == idJardin)
                .Select(x => new SalaResult(x.Id, x.Nombre))
                .ToListAsync();

            return result;
        }

        public async Task<SalaDetalleResult> ObtenerSala(Guid idSala)
        {
            var result = await _context.Set<Sala>()
                .Include(x => x.UsuariosSalasRoles)
                    .ThenInclude(x => x.Usuario)
                    .ThenInclude(x => x.Persona)
                .Where(x => x.Id == idSala)
                .Select(x => new SalaDetalleResult(
                    x.Id,
                    x.Nombre,
                    x.UsuariosSalasRoles.Select(x => new SalaMiembroBasicoResult(x.IdUsuario, x.Usuario.Persona!.Nombre, x.Usuario.Persona.Apellido)).ToList()))
                .FirstOrDefaultAsync();

            if (result == null)
                throw new KeyNotFoundException("Sala no encontrada.");

            return result;
        }

        public async Task<List<SalaMiembroResult>> ObtenerMiembros(Guid idSala)
        {
            var miembros = await _context.Set<UsuarioSalaRol>()
                .Include(x => x.Usuario)
                    .ThenInclude(x => x.Persona)
                .Include(x => x.Usuario)
                    .ThenInclude(x => x.Tutelas)
                        .ThenInclude(t => t.Infante)
                            .ThenInclude(i => i.Salas)
                .Include(x => x.Usuario)
                    .ThenInclude(x => x.Tutelas)
                        .ThenInclude(t => t.TipoTutela)
                .Include(x => x.Rol)
                .Where(x => x.IdSala == idSala)
                .Select(x => new SalaMiembroResult(
                    x.IdUsuario,
                    x.Usuario.Persona!.Nombre,
                    x.Usuario.Persona.Apellido,
                    x.Rol.Descripcion,
                    x.Usuario.Tutelas
                        .Where(t => t.Infante.Salas.Any(s => s.IdSala == idSala))
                        .Select(t => new TutelaInfoResult(
                            t.IdInfante,
                            t.Infante.Nombre,
                            t.Infante.Apellido,
                            t.TipoTutela.Descripcion))
                        .ToList()))
                .ToListAsync();

            return miembros;
        }

        public async Task<bool> CheckSalaPerteneceJardin(Guid idJardin, Guid idSala)
        {
            var check = await _context.Set<Sala>()
                .Where(x => x.Id == idSala && x.IdJardin == idJardin)
                .AnyAsync();

            return check;
        }

        public async Task<bool> CheckUsuarioPerteneceASala(Guid idSala, Guid idUsuario, int? rol = null)
        {
            var check = await _context.Set<UsuarioSalaRol>()
                .Where(x => x.IdSala == idSala && x.IdUsuario == idUsuario && (rol == null || x.IdRol == rol))
                .AnyAsync();

            return check;
        }

        public async Task AsociarEducadorMedianteEmail(Guid salaId, string email)
        {
            var sala = await _context.Set<Sala>()
                .Include(s => s.Jardin)
                .FirstOrDefaultAsync(s => s.Id == salaId);

            if (sala is null) throw new KeyNotFoundException("Sala no encontrada.");

            var usuario = await _context.Set<Usuario>()
                .Include(u => u.Persona)
                .FirstOrDefaultAsync(u => u.Email == email && u.IdJardin == sala.IdJardin && u.DeletedAt == null);

            if (usuario is null) throw new KeyNotFoundException("Usuario no encontrado.");

            var yaMiembro = await _context.Set<UsuarioSalaRol>()
                .AnyAsync(u => u.IdUsuario == usuario.Id && u.IdSala == salaId);

            if (yaMiembro) throw new InvalidOperationException("El usuario ya es miembro de la sala.");

            await _context.AddAsync(new UsuarioSalaRol
            {
                IdUsuario = usuario.Id,
                IdSala = salaId,
                IdRol = (int)RolId.Educador,
                CreatedAt = DateTime.UtcNow
            });

            await _context.SaveChangesAsync();

            await _emailService.SendTemplateAsync(usuario.Email, new AsignacionSalaViewModel
            {
                NombreEducador = usuario.Persona?.Nombre ?? usuario.Email,
                NombreSala = sala.Nombre,
                NombreJardin = sala.Jardin.Nombre
            });
        }

        public async Task DesasociarUsuario(Guid idSala, Guid idUsuario)
        {
            var miembro = await _context.Set<UsuarioSalaRol>()
                .FirstOrDefaultAsync(x => x.IdSala == idSala && x.IdUsuario == idUsuario);

            if (miembro == null) throw new KeyNotFoundException("Miembro no encontrado en la sala.");

            _context.Remove(miembro);
            await _context.SaveChangesAsync();
        }
    }
}
