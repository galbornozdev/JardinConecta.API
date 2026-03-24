using Amazon.S3;
using Amazon.S3.Model;
using JardinConecta.Configurations;
using JardinConecta.Services.Infrastructure;
using Microsoft.Extensions.Options;

namespace JardinConecta.Infrastructure
{
    public class SpacesFileStorageService : IFileStorageService
    {
        private readonly IAmazonS3 _s3Client;
        private readonly string _bucketName;
        private readonly string _baseUrl;

        public string BaseUrl => _baseUrl;

        public SpacesFileStorageService(IOptions<SpacesOptions> options)
        {
            var accessKey = options.Value.AccessKey;
            var secretKey = options.Value.SecretKey;
            var serviceUrl = options.Value.Endpoint;
            var region = options.Value.Region;
            _bucketName = options.Value.BucketName;

            var s3Config = new AmazonS3Config
            {
                ServiceURL = serviceUrl,
                ForcePathStyle = true
            };

            _s3Client = new AmazonS3Client(accessKey, secretKey, s3Config);

            _baseUrl = $"{_bucketName}.{region}.cdn.digitaloceanspaces.com";
            _baseUrl = $"https://{_baseUrl}/";
        }

        public async Task<string> SaveAsync(IFormFile file, string safeFileName)
        {
            using var stream = file.OpenReadStream();

            var request = new PutObjectRequest
            {
                BucketName = _bucketName,
                Key = safeFileName,
                InputStream = stream,
                ContentType = file.ContentType,
                CannedACL = S3CannedACL.PublicRead
            };

            await _s3Client.PutObjectAsync(request);

            return safeFileName;
        }

        public void Delete(string safeFileName)
        {
            var request = new DeleteObjectRequest
            {
                BucketName = _bucketName,
                Key = safeFileName
            };

            _s3Client.DeleteObjectAsync(request).Wait();
        }
    }
}