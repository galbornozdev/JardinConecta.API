namespace JardinConecta.Middleware;

public class HttpLoggingOptions
{
    public bool Enabled { get; set; }
    public int MaxBodyLength { get; set; } = 4096;
}
