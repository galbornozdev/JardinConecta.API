using JardinConecta.Core.Entities;
using JardinConecta.Core.Interfaces;
using JardinConecta.Core.Interfaces.Infrastructure;
using JardinConecta.Infrastructure.Repository;
using Microsoft.EntityFrameworkCore;

namespace JardinConecta.Core.Services
{
    public class SalaNotificationService : ISalaNotificationService
    {
        private readonly ServiceContext _context;
        private readonly INotificationService _notificationService;
        private readonly ILogger<SalaNotificationService> _logger;

        public SalaNotificationService(
            ServiceContext context,
            INotificationService notificationService,
            ILogger<SalaNotificationService> logger)
        {
            _context = context;
            _notificationService = notificationService;
            _logger = logger;
        }

        public async Task NotificarAsync(Guid idSala, string titulo, string body, Guid? excluirUsuario = null, Dictionary<string, string>? data = null, bool soloFamilias = true)
        {
            var tokens = await _context.Set<UsuarioSalaRol>()
                .Where(x => x.IdSala == idSala && x.Usuario.DeviceToken != null)
                .Where(x => excluirUsuario == null || x.IdUsuario != excluirUsuario)
                .Where(x => !soloFamilias || x.IdRol == (int)RolId.Familia)
                .Include(x => x.Usuario)
                .Select(x => x.Usuario.DeviceToken!)
                .ToListAsync();

            foreach (var token in tokens)
            {
                try
                {
                    await _notificationService.SendPushAsync(token, titulo, body, data);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Error enviando push al token {Token}", token);
                }
            }
        }
    }
}
