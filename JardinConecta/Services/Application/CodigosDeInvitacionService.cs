using JardinConecta.Common;
using JardinConecta.Infrastructure.Repository;
using JardinConecta.Models.Entities;
using JardinConecta.Models.Http.Responses;
using JardinConecta.Services.Application.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace JardinConecta.Services.Application
{
    public class CodigosDeInvitacionService : ICodigosDeInvitacionService
    {
        private readonly ServiceContext _context;

        public CodigosDeInvitacionService(
            ServiceContext context
        )
        {
            _context = context;
        }

        public async Task<CodigoInvitacionResponse> GenerarCodigoInvitacionSala(Guid idJardin, Guid idSala, DateTime fechaExpiracion, TipoInvitacion tipoInvitacion, Guid? idInfante = null)
        {
            var sala = await _context.Set<Sala>().Where(s => s.Id == idSala && s.IdJardin == idJardin).FirstOrDefaultAsync();

            if (sala == null) throw new ArgumentException("El identificador de la sala es incorrecto.");

            if (tipoInvitacion == TipoInvitacion.Familia)
            {
                if (idInfante is null) throw new ArgumentException("Debe proporcionarse al menos un identificador de infante cuando el tipo de codigo es destinado a familias.");

                var infante = await _context.Set<Infante>().Where(i => i.Id == idInfante && i.IdJardin == idJardin && i.DeletedAt == null).FirstOrDefaultAsync();
                if (infante is null) throw new ArgumentException("El identificador de infante es incorrecto.");

                var perteneceASala = await _context.Set<InfanteSala>().AnyAsync(x => x.IdInfante == idInfante && x.IdSala == idSala);
                if (!perteneceASala)
                    await _context.AddAsync(new InfanteSala { IdInfante = idInfante.Value, IdSala = idSala });
            }

            string codigo;
            bool colision;
            do
            {
                codigo = Helpers.GenerateRandomStringUpperCase();
                colision = await _context.Set<CodigoInvitacion>().AnyAsync(c => c.Codigo == codigo);
            } while (colision);

            var invitacion = new CodigoInvitacion
            {
                Id = Guid.NewGuid(),
                Codigo = codigo,
                IdSala = idSala,
                IdInfante = idInfante,
                TipoInvitacion = (int)tipoInvitacion,
                FechaExpiracion = fechaExpiracion,
                CreatedAt = DateTime.UtcNow
            };

            await _context.AddAsync(invitacion);
            await _context.SaveChangesAsync();

            return new CodigoInvitacionResponse
            {
                Id = invitacion.Id,
                Codigo = invitacion.Codigo,
                IdSala = invitacion.IdSala,
                IdInfante = invitacion.IdInfante,
                TipoInvitacion = invitacion.TipoInvitacion,
                FechaExpiracion = invitacion.FechaExpiracion
            };
        }

        public async Task<List<CodigoInvitacionItemResponse>> ListarCodigosInvitacion(Guid idJardin, Guid idSala)
        {
            var salaExiste = await _context.Set<Sala>().AnyAsync(s => s.Id == idSala && s.IdJardin == idJardin);
            if (!salaExiste) throw new ArgumentException("El identificador de la sala es incorrecto.");

            var now = DateTime.UtcNow;
            var invitaciones = await _context.Set<CodigoInvitacion>()
                .Where(c => c.IdSala == idSala)
                .Select(c => new CodigoInvitacionItemResponse
                {
                    Id = c.Id,
                    Codigo = c.Codigo,
                    NombreInfante = c.Infante != null ? c.Infante.Nombre + " " + c.Infante.Apellido : null,
                    TipoInvitacion = c.TipoInvitacion,
                    FechaExpiracion = c.FechaExpiracion,
                    EstaVigente = c.FechaExpiracion > now
                })
                .ToListAsync();

            return invitaciones;
        }

        public async Task<VerificarInvitacionResponse> VerificarCodigo(string codigo)
        {
            var now = DateTime.UtcNow;

            var invitacion = await _context.Set<CodigoInvitacion>()
                .Include(c => c.Sala).ThenInclude(s => s.Jardin)
                .Where(c => c.Codigo == codigo && c.FechaExpiracion > now)
                .FirstOrDefaultAsync();

            if (invitacion is null) throw new ArgumentException("El código de invitación es inválido o ha expirado.");

            var tipo = invitacion.TipoInvitacion == (int)TipoInvitacion.Educador
                ? nameof(TipoInvitacion.Educador)
                : nameof(TipoInvitacion.Familia);

            return new VerificarInvitacionResponse
            {
                TipoInvitacion = tipo,
                NombreSala = invitacion.Sala.Nombre,
                NombreJardin = invitacion.Sala.Jardin.Nombre
            };
        }
        public async Task CanjearCodigo(Guid idUsuario, string codigo, string? documentoSufijo = null, int ? idTipoTutela = null)
        {
            var now = DateTime.UtcNow;

            var invitacion = await _context.Set<CodigoInvitacion>()
                .Include(c => c.Infante)
                .Where(c => c.Codigo == codigo && c.FechaExpiracion > now)
                .FirstOrDefaultAsync();

            if (invitacion is null) throw new ArgumentException("El código de invitación es inválido o ha expirado.");

            if (invitacion.TipoInvitacion == (int)TipoInvitacion.Educador)
            {
                var yaMiembro = await _context.Set<UsuarioSalaRol>()
                    .AnyAsync(u => u.IdUsuario == idUsuario && u.IdSala == invitacion.IdSala);

                if (!yaMiembro)
                {
                    await _context.AddAsync(new UsuarioSalaRol
                    {
                        IdUsuario = idUsuario,
                        IdSala = invitacion.IdSala,
                        IdRol = (int)RolId.Educador,
                        CreatedAt = now
                    });
                }
            }
            else
            {
                if (string.IsNullOrWhiteSpace(documentoSufijo) || idTipoTutela is null)
                    throw new ArgumentException("El sufijo de documento del infante es requerido.");

                var documento = invitacion.Infante?.Documento?.Trim();
                if (documento is null || documento.Length < 3) throw new InvalidOperationException("El documento del infante asociado no pudo ser obtenido.");

                var sufijo = documento[^3..];
                if (!sufijo.Equals(documentoSufijo.Trim(), StringComparison.OrdinalIgnoreCase))
                    throw new InvalidOperationException("El sufijo de documento del infante proporcionado es incorrecto.");

                var yaMiembro = await _context.Set<UsuarioSalaRol>()
                    .AnyAsync(u => u.IdUsuario == idUsuario && u.IdSala == invitacion.IdSala);

                if (!yaMiembro)
                {
                    await _context.AddAsync(new UsuarioSalaRol
                    {
                        IdUsuario = idUsuario,
                        IdSala = invitacion.IdSala,
                        IdRol = (int)RolId.Familia,
                        CreatedAt = now
                    });
                }

                var tutela = await _context.Set<Tutela>()
                    .FirstOrDefaultAsync(t => t.IdUsuario == idUsuario && t.IdInfante == invitacion.IdInfante);

                if (tutela is null)
                {
                    await _context.AddAsync(new Tutela
                    {
                        IdUsuario = idUsuario,
                        IdInfante = invitacion.IdInfante!.Value,
                        IdTipoTutela = idTipoTutela.Value,
                        CreatedAt = now
                    });
                }
                else
                {
                    tutela.IdTipoTutela = idTipoTutela.Value;
                }
            }

            await _context.SaveChangesAsync();
        }
    }
}
