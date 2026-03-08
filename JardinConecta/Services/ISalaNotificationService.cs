namespace JardinConecta.Services
{
    public interface ISalaNotificationService
    {
        Task NotificarAsync(Guid idSala, string titulo, string body, Guid? excluirUsuario = null);
    }
}
