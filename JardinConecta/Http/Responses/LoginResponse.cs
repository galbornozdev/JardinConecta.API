namespace JardinConecta.Http.Responses
{
    public class LoginResponse
    {
        public string Token { get; set; } = null!;
        public DateTime Expires { get; set; }
    }
}
