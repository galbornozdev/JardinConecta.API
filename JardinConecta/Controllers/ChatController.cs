using JardinConecta.Infrastructure.Repository;
using JardinConecta.Models.Entities;
using JardinConecta.Models.Http;
using JardinConecta.Models.Http.Requests;
using JardinConecta.Models.Http.Responses;
using JardinConecta.Services.Infrastructure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace JardinConecta.Controllers
{
    [ApiController]
    [Route("[controller]")]
    [Authorize]
    public class ChatController : AbstractController
    {
        private readonly INotificationService _notificationService;

        public ChatController(ServiceContext context, INotificationService notificationService) : base(context)
        {
            _notificationService = notificationService;
        }

        [HttpGet("Conversaciones")]
        [ProducesResponseType(typeof(List<ConversacionItemResponse>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetConversaciones()
        {
            var yo = User.GetIdUsuario();

            var mensajes = await _context.Set<ChatMessage>()
                .Include(m => m.Sala)
                .Where(m => m.IdRemitente == yo || m.IdDestinatario == yo)
                .ToListAsync();

            var agrupados = mensajes
                .GroupBy(m => new
                {
                    IdContacto = m.IdRemitente == yo ? m.IdDestinatario : m.IdRemitente,
                    IdSala = m.IdSala ?? Guid.Empty
                })
                .Select(g => new
                {
                    g.Key.IdContacto,
                    g.Key.IdSala,
                    NombreSala = g.FirstOrDefault(m => m.Sala != null)?.Sala?.Nombre ?? "",
                    UltimoMensaje = g.OrderByDescending(m => m.CreatedAt).First(),
                    NoLeidos = g.Count(m => m.IdDestinatario == yo && m.LeidoAt == null)
                })
                .OrderByDescending(x => x.UltimoMensaje.CreatedAt)
                .ToList();

            var contactIds = agrupados.Select(g => g.IdContacto).Distinct().ToList();
            var contactos = await _context.Set<Usuario>()
                .Include(u => u.Persona)
                .Where(u => contactIds.Contains(u.Id))
                .ToListAsync();

            var result = agrupados.Select(g =>
            {
                var contacto = contactos.First(c => c.Id == g.IdContacto);
                return new ConversacionItemResponse(
                    g.IdContacto,
                    $"{contacto.Persona?.Nombre} {contacto.Persona?.Apellido}".Trim(),
                    contacto.Persona?.PhotoUrl,
                    g.UltimoMensaje.Texto,
                    g.UltimoMensaje.CreatedAt,
                    g.NoLeidos,
                    g.IdSala,
                    g.NombreSala
                );
            }).ToList();

            return Ok(result);
        }

        [HttpGet("Conversaciones/{usuarioId}")]
        [ProducesResponseType(typeof(Pagination<ChatMensajeResponse>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetHistorial(Guid usuarioId, [FromQuery] Guid idSala, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
        {
            var yo = User.GetIdUsuario();

            var query = _context.Set<ChatMessage>()
                .Where(m => m.IdSala == idSala &&
                    ((m.IdRemitente == yo && m.IdDestinatario == usuarioId)
                     || (m.IdRemitente == usuarioId && m.IdDestinatario == yo)))
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

            return Ok(new Pagination<ChatMensajeResponse>(items, totalPages, page, pageSize));
        }

        [HttpPost("Conversaciones/{usuarioId}")]
        [ProducesResponseType(typeof(ChatMensajeResponse), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> EnviarMensaje(Guid usuarioId, EnviarMensajeRequest request)
        {
            var yo = User.GetIdUsuario();

            var mensaje = new ChatMessage
            {
                Id = Guid.NewGuid(),
                IdRemitente = yo,
                IdDestinatario = usuarioId,
                IdSala = request.IdSala,
                Texto = request.Texto,
                CreatedAt = DateTime.UtcNow
            };

            await _context.AddAsync(mensaje);
            await _context.SaveChangesAsync();

            var remitente = await _context.Set<Usuario>()
                .Include(u => u.Persona)
                .Where(u => u.Id == yo)
                .FirstOrDefaultAsync();

            var destinatarioUsuario = await _context.Set<Usuario>()
                .Where(u => u.Id == usuarioId)
                .FirstOrDefaultAsync();

            if (destinatarioUsuario?.DeviceToken != null)
            {
                var remitenteNombre = $"{remitente?.Persona?.Nombre} {remitente?.Persona?.Apellido}".Trim();
                await _notificationService.SendPushAsync(destinatarioUsuario.DeviceToken, remitenteNombre, request.Texto);
            }

            var response = new ChatMensajeResponse(
                mensaje.Id,
                mensaje.IdRemitente,
                mensaje.Texto,
                mensaje.CreatedAt,
                false
            );

            return CreatedAtAction(nameof(GetHistorial), new { usuarioId }, response);
        }

        [HttpPost("Conversaciones/{usuarioId}/Leer")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<IActionResult> MarcarLeidos(Guid usuarioId, [FromQuery] Guid idSala)
        {
            var yo = User.GetIdUsuario();

            await _context.Set<ChatMessage>()
                .Where(m => m.IdSala == idSala && m.IdRemitente == usuarioId && m.IdDestinatario == yo && m.LeidoAt == null)
                .ExecuteUpdateAsync(s => s.SetProperty(m => m.LeidoAt, DateTime.UtcNow));

            return NoContent();
        }

        [HttpGet("Contactos")]
        [ProducesResponseType(typeof(List<ContactoChatResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> GetContactos([FromQuery] Guid idSala)
        {
            var yo = User.GetIdUsuario();

            var rolYo = await _context.Set<UsuarioSalaRol>()
                .Where(x => x.IdUsuario == yo && x.IdSala == idSala
                         && (x.IdRol == (int)RolId.Educador || x.IdRol == (int)RolId.Familia))
                .Select(x => x.IdRol)
                .FirstOrDefaultAsync();

            if (rolYo == 0) return Forbid();

            var rolContactos = rolYo == (int)RolId.Educador ? (int)RolId.Familia : (int)RolId.Educador;

            var rows = await _context.Set<UsuarioSalaRol>()
                .Include(x => x.Usuario).ThenInclude(x => x.Persona)
                .Where(x => x.IdSala == idSala && x.IdRol == rolContactos)
                .ToListAsync();

            var contactos = rows.Select(x => new ContactoChatResponse(
                x.IdUsuario,
                $"{x.Usuario.Persona?.Nombre} {x.Usuario.Persona?.Apellido}".Trim(),
                x.Usuario.Persona?.PhotoUrl))
                .ToList();

            return Ok(contactos);
        }
    }
}
