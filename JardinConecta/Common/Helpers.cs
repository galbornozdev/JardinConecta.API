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
    }
}
