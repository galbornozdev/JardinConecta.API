namespace JardinConecta.Services.Application.Interfaces
{
    public interface ISalaNotificationService
    {
        Task NotificarAsync(Guid idSala, string titulo, string body, Guid? excluirUsuario = null, Dictionary<string, string>? data = null, bool soloFamilias = true);
    }
}
