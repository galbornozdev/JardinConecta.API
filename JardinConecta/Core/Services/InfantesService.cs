using System.Globalization;
using JardinConecta.Core.Entities;
using JardinConecta.Core.Interfaces;
using JardinConecta.Core.Services.Dtos;
using JardinConecta.Infrastructure.Repository;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace JardinConecta.Core.Services
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

        public async Task<List<InfanteResult>> ObtenerInfantes(Guid idJardin, Guid? idSala)
        {
            var query = _context.Set<Infante>()
                .Include(i => i.Salas).ThenInclude(s => s.Sala)
                .Where(i => i.IdJardin == idJardin && i.DeletedAt == null);

            if (idSala.HasValue)
                query = query.Where(i => i.Salas.Any(s => s.IdSala == idSala.Value));

            var result = await query
                .Select(i => new InfanteResult(
                    i.Id,
                    i.Nombre,
                    i.Apellido,
                    i.Documento,
                    i.PhotoUrl,
                    i.FechaNacimiento,
                    i.Salas.Select(s => new InfanteSalaResult(s.IdSala, s.Sala.Nombre)).ToList()
                ))
                .ToListAsync();

            return result;
        }

        public async Task<InfanteDetalleResult> ObtenerInfante(Guid infanteId)
        {
            var infante = await _context.Set<Infante>()
                .Include(i => i.Salas).ThenInclude(s => s.Sala)
                .Include(i => i.Tutelas).ThenInclude(t => t.TipoTutela)
                .Include(i => i.Tutelas).ThenInclude(t => t.Usuario).ThenInclude(u => u.Persona)
                .Where(i => i.Id == infanteId && i.DeletedAt == null)
                .Select(i => new InfanteDetalleResult(
                    i.Id,
                    i.Nombre,
                    i.Apellido,
                    i.Documento,
                    i.PhotoUrl,
                    i.FechaNacimiento,
                    i.Tutelas.Select(t => new InfanteTutelaResult(
                        t.IdUsuario,
                        t.Usuario.Persona!.Nombre,
                        t.Usuario.Persona.Apellido,
                        t.TipoTutela.Descripcion
                    )).ToList(),
                    i.Salas.Select(s => new InfanteSalaResult(s.IdSala, s.Sala.Nombre)).ToList()
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

        public async Task<ImportarInfantesResult> ImportarInfantes(Guid idJardin, IFormFile csvFile)
        {
            var salasJardin = await _context.Set<Sala>()
                .Where(s => s.IdJardin == idJardin)
                .ToListAsync();

            var filas = ParsearCsv(csvFile);

            await using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                int insertados = 0;
                int actualizados = 0;

                for (int i = 0; i < filas.Count; i++)
                {
                    var fila = filas[i];
                    int numeroFila = i + 2;

                    Sala? sala = null;
                    if (!string.IsNullOrWhiteSpace(fila.Sala))
                    {
                        sala = salasJardin.FirstOrDefault(
                            s => s.Nombre.Equals(fila.Sala.Trim(), StringComparison.OrdinalIgnoreCase));
                        if (sala == null)
                            throw new ArgumentException(
                                $"Fila {numeroFila}: sala '{fila.Sala}' no encontrada en el jardín.");
                    }

                    var infante = await _context.Set<Infante>()
                        .Include(inf => inf.Salas)
                        .FirstOrDefaultAsync(inf =>
                            inf.IdJardin == idJardin &&
                            inf.Documento == fila.Documento.ToString() &&
                            inf.DeletedAt == null);

                    if (infante == null)
                    {
                        infante = new Infante
                        {
                            Id = Guid.NewGuid(),
                            IdJardin = idJardin,
                            Nombre = fila.Nombre,
                            Apellido = fila.Apellido,
                            Documento = fila.Documento.ToString(),
                            FechaNacimiento = fila.FechaNacimiento
                        };
                        await _context.AddAsync(infante);
                        insertados++;
                    }
                    else
                    {
                        infante.Nombre = fila.Nombre;
                        infante.Apellido = fila.Apellido;
                        infante.FechaNacimiento = fila.FechaNacimiento;
                        infante.UpdatedAt = DateTime.UtcNow;
                        actualizados++;
                    }

                    if (sala != null)
                    {
                        bool yaAsignado = infante.Salas != null &&
                            infante.Salas.Any(s => s.IdSala == sala.Id);
                        if (!yaAsignado)
                        {
                            await _context.AddAsync(new InfanteSala
                            {
                                IdInfante = infante.Id,
                                IdSala = sala.Id
                            });
                        }
                    }
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
                return new ImportarInfantesResult(insertados, actualizados);
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        private record FilaCsv(string Nombre, string Apellido, int Documento, DateTime FechaNacimiento, string Sala);

        private static List<FilaCsv> ParsearCsv(IFormFile csvFile)
        {
            var filas = new List<FilaCsv>();

            using var reader = new StreamReader(csvFile.OpenReadStream());

            var header = reader.ReadLine();
            if (header == null)
                throw new ArgumentException("El archivo CSV está vacío.");

            int numeroFila = 1;
            string? linea;
            while ((linea = reader.ReadLine()) != null)
            {
                numeroFila++;
                if (string.IsNullOrWhiteSpace(linea)) continue;
                if (linea.Trim(';', ' ').Length == 0) continue;

                var partes = linea.Split(';');
                if (partes.Length < 4)
                    throw new ArgumentException(
                        $"Fila {numeroFila}: se esperan al menos 4 columnas (nombre,apellido,documento,fechaNacimiento).");

                string nombre = partes[0].Trim();
                string apellido = partes[1].Trim();
                string documentoStr = partes[2].Trim();
                string fechaStr = partes[3].Trim();
                string sala = partes.Length >= 5 ? partes[4].Trim() : string.Empty;

                if (string.IsNullOrWhiteSpace(nombre))
                    throw new ArgumentException($"Fila {numeroFila}: el nombre es requerido.");

                if (string.IsNullOrWhiteSpace(apellido))
                    throw new ArgumentException($"Fila {numeroFila}: el apellido es requerido.");

                if (!int.TryParse(documentoStr, out int documento))
                    throw new ArgumentException(
                        $"Fila {numeroFila}: documento '{documentoStr}' no es un número válido.");

                if (!DateTime.TryParseExact(fechaStr, new[] { "d/M/yyyy", "yyyy-MM-dd" },
                        CultureInfo.InvariantCulture,
                        DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal,
                        out DateTime fechaNacimiento))
                    throw new ArgumentException(
                        $"Fila {numeroFila}: fecha '{fechaStr}' no tiene formato válido (d/M/yyyy).");

                filas.Add(new FilaCsv(nombre, apellido, documento, fechaNacimiento, sala));
            }

            if (filas.Count == 0)
                throw new ArgumentException("El archivo CSV no contiene filas de datos.");

            return filas;
        }
    }
}
