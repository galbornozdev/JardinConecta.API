using JardinConecta.Common;
using Serilog.Events;
using System.Diagnostics;
using System.Text;
using System.Text.Json.Nodes;

namespace JardinConecta.Middleware;

public class HttpLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly HttpLoggingOptions _options;

    private static readonly HashSet<string> _excludedPrefixes = new(StringComparer.OrdinalIgnoreCase)
    {
        "/Health",
        "/media"
    };

    private static readonly Dictionary<string, string[]> _sensitiveFields = new(StringComparer.OrdinalIgnoreCase)
    {
        { "/Auth/Login",                       ["password"]    },
        { "/Usuarios/RegistrarDispositivo",    ["deviceToken"] }
    };

    public HttpLoggingMiddleware(RequestDelegate next, HttpLoggingOptions options)
    {
        _next = next;
        _options = options;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var path = context.Request.Path.Value ?? string.Empty;

        if (_excludedPrefixes.Any(p => path.StartsWith(p, StringComparison.OrdinalIgnoreCase)))
        {
            await _next(context);
            return;
        }

        context.Request.EnableBuffering();

        var requestBody = await ReadRequestBodyAsync(context.Request);
        var sanitizedRequestBody = SanitizeBody(path, requestBody);

        var originalResponseBody = context.Response.Body;
        using var responseBuffer = new MemoryStream();
        context.Response.Body = responseBuffer;

        var clientIp = context.Request.Headers["X-Forwarded-For"].FirstOrDefault()
            ?? context.Connection.RemoteIpAddress?.ToString();

        var stopwatch = Stopwatch.StartNew();

        try
        {
            await _next(context);
        }
        finally
        {
            stopwatch.Stop();

            responseBuffer.Position = 0;
            var responseBody = await new StreamReader(responseBuffer, leaveOpen: true).ReadToEndAsync();

            responseBuffer.Position = 0;
            await responseBuffer.CopyToAsync(originalResponseBody);
            context.Response.Body = originalResponseBody;

            var statusCode = context.Response.StatusCode;
            var userId = context.User.FindFirst(Constants.CUSTOM_CLAIMS__ID_USUARIO)?.Value;
            var level = statusCode >= 400 ? LogEventLevel.Warning : LogEventLevel.Information;

            Serilog.Log.Logger
                .ForContext("TraceId",       context.TraceIdentifier)
                .ForContext("Method",        context.Request.Method)
                .ForContext("Path",          path)
                .ForContext("QueryString",   context.Request.QueryString.ToString())
                .ForContext("StatusCode",    statusCode)
                .ForContext("DurationMs",    stopwatch.ElapsedMilliseconds)
                .ForContext("ClientIp",      clientIp)
                .ForContext("UserId",        userId)
                .ForContext("RequestBody",   Truncate(sanitizedRequestBody))
                .ForContext("ResponseBody",  Truncate(responseBody))
                .Write(level, "HTTP {Method} {Path} {StatusCode} in {DurationMs}ms");
        }
    }

    private static async Task<string> ReadRequestBodyAsync(HttpRequest request)
    {
        if (!request.Body.CanRead || request.ContentLength == 0)
            return string.Empty;

        request.Body.Position = 0;
        var body = await new StreamReader(request.Body, Encoding.UTF8, leaveOpen: true).ReadToEndAsync();
        request.Body.Position = 0;

        return body;
    }

    private string Truncate(string body)
    {
        if (string.IsNullOrEmpty(body) || body.Length <= _options.MaxBodyLength)
            return body;

        return string.Concat(body.AsSpan(0, _options.MaxBodyLength), "...[truncated]");
    }

    private static string SanitizeBody(string path, string body)
    {
        if (string.IsNullOrWhiteSpace(body)) return body;
        if (!_sensitiveFields.TryGetValue(path, out var fields)) return body;

        try
        {
            var node = JsonNode.Parse(body);
            if (node is not JsonObject obj) return body;

            foreach (var field in fields)
            {
                if (obj.ContainsKey(field))
                    obj[field] = "***";
            }

            return obj.ToJsonString();
        }
        catch
        {
            return body;
        }
    }
}
