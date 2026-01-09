using JardinConecta.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;
using System.Security.Claims;
using System.Xml;

namespace JardinConecta.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class TestController : ControllerBase
    {
        private readonly IMongoDatabase mongoDatabase;
        private readonly IMongoCollection<TestEntity> _collection;

        public TestController(IMongoDatabase mongoDatabase)
        {
            this.mongoDatabase = mongoDatabase;
            _collection = mongoDatabase.GetCollection<TestEntity>("tests");
        }

        [HttpGet("TestGuid")]
        public string TestGuid()
        {
            return Guid.NewGuid().ToString();
        }

        [HttpPost("TestHash")]
        public string TestHash(string str)
        {
            return PasswordHasher.Hash(str);
        }

        [Authorize]
        [HttpPost("TestSecurity")]
        public IActionResult TestSecurity()
        {
            return Ok();
        }

        [Authorize]
        [HttpPost("ReadClaims")]
        public IActionResult ReadClaims()
        {
            var userId = User.FindFirst("uid")?.Value;
            var email = User.FindFirstValue(ClaimTypes.Email);
            var role = User.FindFirstValue(ClaimTypes.Role);

            return Ok(new { userId, email, role });
        }

        [Authorize(Roles = "AdminSistema")]
        [HttpPost("TestSecurityRoles")]
        public IActionResult TestSecurityRoles()
        {
            return Ok();
        }

        public class TestEntity {
            [BsonId]
            [BsonRepresentation(BsonType.ObjectId)]
            public string? Id { get; set; }

            // Fields
            [BsonElement("name")]
            public string Name { get; set; } = null!;

            [BsonElement("createdAt")]
            [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
            public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        }

        [HttpGet("MongoTests")]
        public async Task<IActionResult> GetAllAsync()
        {
            var tests = await _collection.Find(_ => true).ToListAsync();
            return Ok(tests);
        }

        [HttpGet("MongoTests/{id}")]
        public async Task<IActionResult?> GetByIdAsync(string id)
        {
            var test = await _collection.Find(x => x.Id == id).FirstOrDefaultAsync();

            if(test == null)
                return NotFound();

            return Ok(test);
        }

        [HttpPost("MongoTests")]
        public async Task<IActionResult> CreateAsync(TestEntity entity)
        {
            await _collection.InsertOneAsync(entity);
            return Ok(entity);
        }

        [HttpPut("MongoTests/{id}")]
        public async Task<IActionResult> UpdateAsync(string id, TestEntity updated)
        {
            var result = await _collection.ReplaceOneAsync(x => x.Id == id, updated);
            var modified = result.IsAcknowledged && result.ModifiedCount > 0;

            return Ok(modified);
        }
         
        [HttpDelete("MongoTests/{id}")]
        public async Task<IActionResult> DeleteAsync(string id)
        {
            var result = await _collection.DeleteOneAsync(x => x.Id == id);
            var deleted = result.IsAcknowledged && result.DeletedCount > 0;

            return Ok(deleted);
        }

        [HttpPost("UploadFile")]
        public async Task<IActionResult> UploadFile(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest();

            //validate extentions
            IList<string> validExtentions = [".jpg" , ".png"];
            var fileExtension = Path.GetExtension(file.FileName);

            if (!validExtentions.Contains(fileExtension))
                return BadRequest();

            //validate file size
            //if(file.Length > (5 * 1024 * 1024)) //5MB
            //    return BadRequest();

            
            var fileName = Path.GetFileNameWithoutExtension(Path.GetRandomFileName()) + "_" + DateTime.UtcNow.ToString("yyyyMMddHHmmssfff") + fileExtension ;

            var uploadDirectory = Path.Combine(Directory.GetCurrentDirectory(), "Uploads");
            var filePath = Path.Combine(uploadDirectory, fileName);

            if (!Directory.Exists(uploadDirectory))
                Directory.CreateDirectory(uploadDirectory);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            return Ok();
        }
    }
}
