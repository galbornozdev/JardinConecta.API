
namespace JardinConecta.Services
{
    public interface IFileStorageService
    {
        string BaseUrl { get; }

        Task<string> SaveAsync(IFormFile file, string safeFileName);
    }
}