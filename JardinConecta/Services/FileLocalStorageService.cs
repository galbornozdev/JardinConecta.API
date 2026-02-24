namespace JardinConecta.Services
{
    public class FileLocalStorageService : IFileStorageService
    {
        public const string UPLOADS_FOLDER = "uploads";
        public async Task<string> SaveAsync(IFormFile file, string safeFileName)
        {
            var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), UPLOADS_FOLDER);

            if (!Directory.Exists(uploadsFolder))
                Directory.CreateDirectory(uploadsFolder);

            var filePath = Path.Combine(uploadsFolder, safeFileName);

            await using var stream = new FileStream(filePath, FileMode.Create);
            await file.CopyToAsync(stream);

            return safeFileName;
        }
    }
}
