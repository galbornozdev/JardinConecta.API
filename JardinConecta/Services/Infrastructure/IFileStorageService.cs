namespace JardinConecta.Services.Infrastructure
{
    public interface IFileStorageService
    {
        string BaseUrl { get; }
        Task<string> SaveAsync(IFormFile file, string safeFileName);
        void Delete(string safeFileName);
    }
}
