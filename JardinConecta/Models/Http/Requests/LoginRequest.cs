using System.ComponentModel.DataAnnotations;

namespace JardinConecta.Models.Http.Requests
{
    public class LoginRequest
    {
        [Required]
        public string Email { get; init; } = null!;

        [Required]
        public string Password { get; init; } = null!;
    }
}
