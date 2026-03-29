namespace JardinConecta.Core.Interfaces.Infrastructure
{
    public interface IFileStorageService
    {
        string BaseUrl { get; }
        Task<string> SaveAsync(IFormFile file, string safeFileName);
        void Delete(string safeFileName);
    }
}
