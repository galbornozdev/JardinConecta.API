using JardinConecta.Common;
using JardinConecta.Core.Entities;
using JardinConecta.Core.Interfaces;
using JardinConecta.Core.Services.Dtos;
using JardinConecta.Infrastructure.Repository;
using Microsoft.EntityFrameworkCore;

namespace JardinConecta.Core.Services
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

        public async Task<CodigoInvitacionResult> GenerarCodigoInvitacionSala(Guid idJardin, Guid idSala, DateTime fechaExpiracion, TipoInvitacion tipoInvitacion, List<Guid>? idsInfante = null)
        {
            var sala = await _context.Set<Sala>().Where(s => s.Id == idSala && s.IdJardin == idJardin).FirstOrDefaultAsync();

            if (sala == null) throw new ArgumentException("El identificador de la sala es incorrecto.");

            if (tipoInvitacion == TipoInvitacion.Familia)
            {
                if (idsInfante is null || idsInfante.Count == 0)
                    throw new ArgumentException("Debe proporcionarse al menos un identificador de infante cuando el tipo de codigo es destinado a familias.");

                foreach (var idInfante in idsInfante)
                {
                    var infante = await _context.Set<Infante>().Where(i => i.Id == idInfante && i.IdJardin == idJardin && i.DeletedAt == null).FirstOrDefaultAsync();
                    if (infante is null) throw new ArgumentException($"El identificador de infante '{idInfante}' es incorrecto.");

                    var perteneceASala = await _context.Set<InfanteSala>().AnyAsync(x => x.IdInfante == idInfante && x.IdSala == idSala);
                    if (!perteneceASala)
                        await _context.AddAsync(new InfanteSala { IdInfante = idInfante, IdSala = idSala });
                }
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
                TipoInvitacion = (int)tipoInvitacion,
                FechaExpiracion = fechaExpiracion,
                CreatedAt = DateTime.UtcNow
            };

            await _context.AddAsync(invitacion);

            if (idsInfante is not null)
            {
                foreach (var idInfante in idsInfante)
                {
                    await _context.AddAsync(new CodigoInvitacionInfante
                    {
                        IdCodigoInvitacion = invitacion.Id,
                        IdInfante = idInfante
                    });
                }
            }

            await _context.SaveChangesAsync();

            return new CodigoInvitacionResult(
                invitacion.Id,
                invitacion.Codigo,
                invitacion.IdSala,
                idsInfante ?? [],
                invitacion.TipoInvitacion,
                invitacion.FechaExpiracion
            );
        }

        public async Task<List<CodigoInvitacionItemResult>> ListarCodigosInvitacion(Guid idJardin, Guid idSala)
        {
            var salaExiste = await _context.Set<Sala>().AnyAsync(s => s.Id == idSala && s.IdJardin == idJardin);
            if (!salaExiste) throw new ArgumentException("El identificador de la sala es incorrecto.");

            var now = DateTime.UtcNow;
            var invitaciones = await _context.Set<CodigoInvitacion>()
                .Include(c => c.Infantes).ThenInclude(x => x.Infante)
                .Where(c => c.IdSala == idSala)
                .Select(c => new CodigoInvitacionItemResult(
                    c.Id,
                    c.Codigo,
                    c.Infantes.Select(x => x.Infante.Nombre + " " + x.Infante.Apellido).ToList(),
                    c.TipoInvitacion,
                    c.FechaExpiracion,
                    c.FechaExpiracion > now
                ))
                .ToListAsync();

            return invitaciones;
        }

        public async Task<VerificarInvitacionResult> VerificarCodigo(string codigo)
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

            return new VerificarInvitacionResult(tipo, invitacion.Sala.Nombre, invitacion.Sala.Jardin.Nombre);
        }

        public async Task CanjearCodigo(Guid idUsuario, string codigo, string? documentoSufijo = null, int? idTipoTutela = null)
        {
            var now = DateTime.UtcNow;

            var invitacion = await _context.Set<CodigoInvitacion>()
                .Include(c => c.Infantes).ThenInclude(x => x.Infante)
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

                var coincide = invitacion.Infantes.Any(x =>
                {
                    var documento = x.Infante?.Documento?.Trim();
                    return documento is not null && documento.Length >= 3 &&
                           documento[^3..].Equals(documentoSufijo.Trim(), StringComparison.OrdinalIgnoreCase);
                });

                if (!coincide)
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

                foreach (var item in invitacion.Infantes)
                {
                    var tutela = await _context.Set<Tutela>()
                        .FirstOrDefaultAsync(t => t.IdUsuario == idUsuario && t.IdInfante == item.IdInfante);

                    if (tutela is null)
                    {
                        await _context.AddAsync(new Tutela
                        {
                            IdUsuario = idUsuario,
                            IdInfante = item.IdInfante,
                            IdTipoTutela = idTipoTutela.Value,
                            CreatedAt = now
                        });
                    }
                    else
                    {
                        tutela.IdTipoTutela = idTipoTutela.Value;
                    }
                }
            }

            await _context.SaveChangesAsync();
        }
    }
}
