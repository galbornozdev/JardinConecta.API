using JardinConecta.Infrastructure.Repository;
using JardinConecta.Models.Entities;
using JardinConecta.Models.Http.Responses;
using JardinConecta.Services.Application.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace JardinConecta.Services.Application
{
    public class InfantesService : IInfantesService
    {
        private readonly ServiceContext _context;

        public InfantesService(
            ServiceContext context
        )
        {
            _context = context;
        }
        public async Task AltaDeInfante(Guid idJardin, string nombre, string apellido, int documento, DateTime fechaNacimiento)
        {
            bool existeInfante = await _context.Set<Infante>()
                .Where(i => i.IdJardin == idJardin && i.Documento == documento.ToString() && i.DeletedAt == null)
                .AnyAsync();

            if (existeInfante) throw new InvalidOperationException("Ya existe el infante.");

            var infante = new Infante()
            {
                Id = Guid.NewGuid(),
                IdJardin = idJardin,
                Nombre = nombre,
                Apellido = apellido,
                Documento = documento.ToString(),
                FechaNacimiento = fechaNacimiento
            };

            await _context.AddAsync(infante);
            await _context.SaveChangesAsync();
        }

        public async Task ActualizarInfante(Guid idInfante, string nombre, string apellido, int documento, DateTime fechaNacimiento)
        {
            var infante = await _context.Set<Infante>().FirstOrDefaultAsync(i => i.Id == idInfante && i.DeletedAt == null);

            if (infante == null) throw new KeyNotFoundException("No existe el infante.");

            infante.Nombre = nombre;
            infante.Apellido = apellido;
            infante.Documento = documento.ToString();
            infante.FechaNacimiento = fechaNacimiento;
            infante.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
        }

        public async Task EliminarInfante(Guid idInfante)
        {
            var infante = await _context.Set<Infante>().FirstOrDefaultAsync(i => i.Id == idInfante && i.DeletedAt == null);

            if (infante == null) throw new KeyNotFoundException("No existe el infante.");

            bool tieneSalas = await _context.Set<InfanteSala>().AnyAsync(x => x.IdInfante == idInfante);

            if (tieneSalas) throw new InvalidOperationException("El infante esta asociado a una sala");

            infante.DeletedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
        }

        public async Task<List<InfantesResponse>> ObtenerInfantes(Guid idJardin, Guid? idSala)
        {
            var query = _context.Set<Infante>()
                .Include(i => i.Salas).ThenInclude(s => s.Sala)
                .Where(i => i.IdJardin == idJardin && i.DeletedAt == null);

            if (idSala.HasValue)
                query = query.Where(i => i.Salas.Any(s => s.IdSala == idSala.Value));

            var result = await query
                .Select(i => new InfantesResponse(
                    i.Id,
                    i.Nombre,
                    i.Apellido,
                    i.Documento,
                    i.PhotoUrl,
                    i.FechaNacimiento,
                    i.Salas.Select(s => new InfantesResponse_Sala(s.IdSala, s.Sala.Nombre)).ToList()
                ))
                .ToListAsync();

            return result;
        }

        public async Task<InfanteDetalleResponse> ObtenerInfante(Guid infanteId)
        {
            var infante = await _context.Set<Infante>()
                .Include(i => i.Salas).ThenInclude(s => s.Sala)
                .Include(i => i.Tutelas).ThenInclude(t => t.TipoTutela)
                .Include(i => i.Tutelas).ThenInclude(t => t.Usuario).ThenInclude(u => u.Persona)
                .Where(i => i.Id == infanteId && i.DeletedAt == null)
                .Select(i => new InfanteDetalleResponse(
                    i.Id,
                    i.Nombre,
                    i.Apellido,
                    i.Documento,
                    i.PhotoUrl,
                    i.FechaNacimiento,
                    i.Tutelas.Select(t => new InfanteDetalleResponse_Tutela(
                        t.IdUsuario,
                        t.Usuario.Persona!.Nombre,
                        t.Usuario.Persona.Apellido,
                        t.TipoTutela.Descripcion
                    )).ToList(),
                    i.Salas.Select(s => new InfantesResponse_Sala(s.IdSala, s.Sala.Nombre)).ToList()
                ))
                .FirstOrDefaultAsync();

            if (infante == null) throw new KeyNotFoundException("No existe el infante.");

            return infante;
        }

        public async Task AsignarSala(Guid idInfante, Guid idSala)
        {
            var infante = await _context.Set<Infante>().FirstOrDefaultAsync(i => i.Id == idInfante && i.DeletedAt == null);
            if (infante == null) throw new KeyNotFoundException("No se encuentra infante");

            var sala = await _context.Set<Sala>().FindAsync(idSala);
            if (sala == null) throw new KeyNotFoundException("No se encuentra sala");

            bool yaAsignado = await _context.Set<InfanteSala>()
                .AnyAsync(x => x.IdInfante == idInfante && x.IdSala == idSala);

            if (yaAsignado) return;

            await _context.AddAsync(new InfanteSala { IdInfante = idInfante, IdSala = idSala });
            await _context.SaveChangesAsync();
        }

        public async Task DesasignarSala(Guid idInfante, Guid idSala)
        {
            var asignacion = await _context.Set<InfanteSala>()
                .FirstOrDefaultAsync(x => x.IdInfante == idInfante && x.IdSala == idSala);

            if (asignacion == null) return;

            _context.Remove(asignacion);
            await _context.SaveChangesAsync();
        }

        public async Task DesasignarTutela(Guid infanteId, Guid usuarioId)
        {
            var tutela = await _context.Set<Tutela>()
                .FirstOrDefaultAsync(t => t.IdInfante == infanteId && t.IdUsuario == usuarioId);

            if (tutela == null) return;

            _context.Remove(tutela);
            await _context.SaveChangesAsync();
        }
    }
}
