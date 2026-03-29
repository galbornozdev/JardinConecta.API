using JardinConecta.Infrastructure.Repository;
using JardinConecta.Models.Entities;
using JardinConecta.Models.Http.Responses;
using JardinConecta.Services.Application.Interfaces;
using JardinConecta.Services.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace JardinConecta.Services.Application
{
    public class ChatService : IChatService
    {
        private readonly ServiceContext _context;
        private readonly IFileStorageService _fileStorageService;
        private readonly INotificationService _notificationService;

        public ChatService(
            ServiceContext context,
            IFileStorageService fileStorageService,
            INotificationService notificationService
        )
        {
            _context = context;
            _fileStorageService = fileStorageService;
            _notificationService = notificationService;
        }

        public async Task<List<ConversacionItemResponse>> ObtenerConversaciones(Guid IdUsuarioLogueado)
        {
            var mensajes = await _context.Set<ChatMessage>()
                .Include(m => m.Sala)
                .Where(m => m.IdRemitente == IdUsuarioLogueado || m.IdDestinatario == IdUsuarioLogueado)
                .ToListAsync();

            var agrupados = mensajes
                .GroupBy(m => new
                {
                    IdContacto = m.IdRemitente == IdUsuarioLogueado ? m.IdDestinatario : m.IdRemitente,
                    IdSala = m.IdSala ?? Guid.Empty
                })
                .Select(g => new
                {
                    g.Key.IdContacto,
                    g.Key.IdSala,
                    NombreSala = g.FirstOrDefault(m => m.Sala != null)?.Sala?.Nombre ?? "",
                    UltimoMensaje = g.OrderByDescending(m => m.CreatedAt).First(),
                    NoLeidos = g.Count(m => m.IdDestinatario == IdUsuarioLogueado && m.LeidoAt == null)
                })
                .OrderByDescending(x => x.UltimoMensaje.CreatedAt)
                .ToList();

            var contactIds = agrupados.Select(g => g.IdContacto).Distinct().ToList();
            var contactos = await _context.Set<Usuario>()
                .Include(u => u.Persona)
                .Where(u => contactIds.Contains(u.Id))
                .ToListAsync();

            var conversaciones = agrupados.Select(g =>
            {
                var contacto = contactos.First(c => c.Id == g.IdContacto);
                return new ConversacionItemResponse(
                    g.IdContacto,
                    $"{contacto.Persona?.Nombre} {contacto.Persona?.Apellido}".Trim(),
                    string.IsNullOrEmpty(contacto.Persona?.PhotoUrl) ? null : _fileStorageService.BaseUrl + contacto.Persona?.PhotoUrl,
                    g.UltimoMensaje.Texto,
                    g.UltimoMensaje.CreatedAt,
                    g.NoLeidos,
                    g.IdSala,
                    g.NombreSala
                );
            }).ToList();

            return conversaciones;
        }

        public async Task<Pagination<ChatMensajeResponse>> ObtenerMensajes(Guid IdUsuarioLogueado, Guid IdUsuarioContraparte, Guid idSala, int page = 1, int pageSize = 20)
        {
            var query = _context.Set<ChatMessage>()
                .Where(m => m.IdSala == idSala &&
                    ((m.IdRemitente == IdUsuarioLogueado && m.IdDestinatario == IdUsuarioContraparte)
                     || (m.IdRemitente == IdUsuarioContraparte && m.IdDestinatario == IdUsuarioLogueado)))
                .OrderByDescending(m => m.CreatedAt);

            var total = await query.CountAsync();
            var totalPages = (int)Math.Ceiling(total / (double)pageSize);

            var items = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(m => new ChatMensajeResponse(
                    m.Id,
                    m.IdRemitente,
                    m.Texto,
                    m.CreatedAt,
                    m.LeidoAt != null
                ))
                .ToListAsync();

            return new Pagination<ChatMensajeResponse>(items, totalPages, page, pageSize);
        }

        public async Task<ChatMensajeResponse> EnviarMensaje(Guid IdUsuarioLogueado, Guid IdUsuarioContraparte, Guid idSala, string texto)
        {
            var mensaje = new ChatMessage
            {
                Id = Guid.NewGuid(),
                IdRemitente = IdUsuarioLogueado,
                IdDestinatario = IdUsuarioContraparte,
                IdSala = idSala,
                Texto = texto,
                CreatedAt = DateTime.UtcNow
            };

            await _context.AddAsync(mensaje);
            await _context.SaveChangesAsync();

            var remitente = await _context.Set<Usuario>()
                .Include(u => u.Persona)
                .Where(u => u.Id == IdUsuarioLogueado)
                .FirstOrDefaultAsync();

            var destinatarioUsuario = await _context.Set<Usuario>()
                .Where(u => u.Id == IdUsuarioContraparte)
                .FirstOrDefaultAsync();

            if (destinatarioUsuario?.DeviceToken != null)
            {
                var remitenteNombre = $"{remitente?.Persona?.Nombre} {remitente?.Persona?.Apellido}".Trim();
                await _notificationService.SendPushAsync(
                    destinatarioUsuario.DeviceToken,
                    remitenteNombre,
                    texto,
                    new Dictionary<string, string> { { "type", "chat" }, { "senderId", IdUsuarioLogueado.ToString() } }
                );
            }

            var result = new ChatMensajeResponse(
                mensaje.Id,
                mensaje.IdRemitente,
                mensaje.Texto,
                mensaje.CreatedAt,
                false
            );

            return result;
        }

        public async Task MarcarMensajesComoLeidos(Guid IdUsuarioLogueado, Guid IdUsuarioContraparte, Guid idSala)
        {
            await _context.Set<ChatMessage>()
                .Where(m => m.IdSala == idSala && m.IdRemitente == IdUsuarioContraparte && m.IdDestinatario == IdUsuarioLogueado && m.LeidoAt == null)
                .ExecuteUpdateAsync(s => s.SetProperty(m => m.LeidoAt, DateTime.UtcNow));
        }

        public async Task<List<ContactoChatResponse>> ObtenerContactos(Guid IdUsuarioLogueado, Guid idSala)
        {
            var rolUsuarioLogueado = await _context.Set<UsuarioSalaRol>()
                .Where(x => x.IdUsuario == IdUsuarioLogueado && x.IdSala == idSala
                         && (x.IdRol == (int)RolId.Educador || x.IdRol == (int)RolId.Familia))
                .Select(x => x.IdRol)
                .FirstOrDefaultAsync();

            if (rolUsuarioLogueado == 0) throw new InvalidOperationException("El usuario no tiene un rol asignado.");

            var rolContactos = rolUsuarioLogueado == (int)RolId.Educador ? (int)RolId.Familia : (int)RolId.Educador;

            var rows = await _context.Set<UsuarioSalaRol>()
                .Include(x => x.Usuario).ThenInclude(x => x.Persona)
                .Where(x => x.IdSala == idSala && x.IdRol == rolContactos)
                .ToListAsync();

            var contactos = rows.Select(x => new ContactoChatResponse(
                x.IdUsuario,
                $"{x.Usuario.Persona?.Nombre} {x.Usuario.Persona?.Apellido}".Trim(),
                string.IsNullOrEmpty(x.Usuario.Persona?.PhotoUrl) ? null : _fileStorageService.BaseUrl + x.Usuario.Persona?.PhotoUrl
                ))
                .ToList();

            return contactos;
        }
    }
}
