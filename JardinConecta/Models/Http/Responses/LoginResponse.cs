namespace JardinConecta.Models.Http.Responses
{
    public class LoginResponse
    {
        public string Token { get; set; } = null!;
        public DateTime Expires { get; set; }
    }
}
