namespace JardinConecta.Services
{
    public static class PasswordHasher
    {
        public static string Hash(string password)
        {
            return BCrypt.Net.BCrypt.EnhancedHashPassword(password);
        }

        public static bool Verify(string inputPassword, string passwordHash)
        {
            return BCrypt.Net.BCrypt.EnhancedVerify(inputPassword, passwordHash);
        }
    }
}
