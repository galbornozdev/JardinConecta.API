
namespace JardinConecta.Services
{
    public interface IFileStorageService
    {
        Task<string> SaveAsync(IFormFile file, string safeFileName);
    }
}