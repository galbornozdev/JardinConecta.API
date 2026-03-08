using System.Security.Cryptography;

namespace JardinConecta.Common
{
    public static class Helpers
    {
        public static string Limit(string text, int maxLength)
        {
            return text.Length <= maxLength
                ? text
                : text.Substring(0, maxLength) + "...";
        }

        public static string GenerateRandomString(int length = 20, string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789")
        {
            var bytes = RandomNumberGenerator.GetBytes(length);
            return new string(bytes.Select(b => chars[b % chars.Length]).ToArray());
        }
        public static string GenerateRandomStringUpperCase(int length = 8, string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789")
        {
            var bytes = RandomNumberGenerator.GetBytes(length);
            return new string(bytes.Select(b => chars[b % chars.Length]).ToArray());
        }
    }
}
