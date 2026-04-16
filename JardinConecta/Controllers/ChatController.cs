using JardinConecta.Core.Entities;
using JardinConecta.Core.Interfaces;
using JardinConecta.Core.Services.Dtos;
using JardinConecta.Models.Http.Requests;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace JardinConecta.Controllers
{
    [ApiController]
    [Route("[controller]")]
    [Authorize]
    public class ChatController : ControllerBase
    {
        private readonly IChatService _chatService;

        public ChatController(
            IChatService chatService
        )
        {
            _chatService = chatService;
        }

        [HttpGet("Conversaciones")]
        [ProducesResponseType(typeof(List<ConversacionItemResult>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetConversaciones()
        {
            var yo = User.GetIdUsuario();

            var result = await _chatService.ObtenerConversaciones(yo);

            return Ok(result);
        }

        [HttpGet("Conversaciones/{usuarioId}")]
        [ProducesResponseType(typeof(PagedResult<ChatMensajeResult>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetHistorial(Guid usuarioId, [FromQuery] Guid idSala, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
        {
            var yo = User.GetIdUsuario();

            var result = await _chatService.ObtenerMensajes(yo, usuarioId, idSala, page, pageSize);

            return Ok(result);
        }

        [HttpPost("Conversaciones/{usuarioId}")]
        [ProducesResponseType(typeof(ChatMensajeResult), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> EnviarMensaje(Guid usuarioId, EnviarMensajeRequest request)
        {
            var yo = User.GetIdUsuario();

            var response = await _chatService.EnviarMensaje(yo, usuarioId, request.IdSala, request.Texto);

            return CreatedAtAction(nameof(GetHistorial), new { usuarioId }, response);
        }

        [HttpPost("Conversaciones/{usuarioId}/Leer")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<IActionResult> MarcarLeidos(Guid usuarioId, [FromQuery] Guid idSala)
        {
            var yo = User.GetIdUsuario();

            await _chatService.MarcarMensajesComoLeidos(yo, usuarioId, idSala);

            return NoContent();
        }

        [HttpGet("Contactos")]
        [ProducesResponseType(typeof(List<ContactoChatResult>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> GetContactos([FromQuery] Guid idSala)
        {
            var yo = User.GetIdUsuario();
            var tipoUsuario = (TipoUsuarioId)User.GetTipoUsuario();

            var contactos = await _chatService.ObtenerContactos(yo, idSala, tipoUsuario);

            return Ok(contactos);
        }
    }
}
