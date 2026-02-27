namespace JardinConecta.Services
{
    public class FileLocalStorageService : IFileStorageService
    {
        private string _uploadsPath;
        private string _uploadsFolder;
        private readonly IHttpContextAccessor _httpContextAccessor;


        public string BaseUrl
        {
            get
            {
                var request = _httpContextAccessor.HttpContext?.Request;
                return request == null
                    ? string.Empty
                    : $"{request.Scheme}://{request.Host}/{_uploadsFolder}/";
            }
        }

        public FileLocalStorageService(IWebHostEnvironment env, IHttpContextAccessor httpContextAccessor)
        {
            _uploadsFolder = "media";
            _uploadsPath = Path.Combine(env.WebRootPath, _uploadsFolder);

            if (!Directory.Exists(_uploadsPath))
                Directory.CreateDirectory(_uploadsPath);
            _httpContextAccessor = httpContextAccessor;
        }
        public async Task<string> SaveAsync(IFormFile file, string safeFileName)
        {
            var filePath = Path.Combine(_uploadsPath, safeFileName);

            await using var stream = new FileStream(filePath, FileMode.Create);
            await file.CopyToAsync(stream);

            return safeFileName;
        }
    }
}
