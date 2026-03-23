using Serilog.Context;

namespace JardinConecta.Middleware;

public class ErrorHandlingMiddleware
{
    private readonly RequestDelegate _next;

    public ErrorHandlingMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        using (LogContext.PushProperty("TraceId", context.TraceIdentifier))
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                Serilog.Log.Logger
                    .ForContext("TraceId", context.TraceIdentifier)
                    .ForContext("Method",  context.Request.Method)
                    .ForContext("Path",    context.Request.Path.Value)
                    .Error(ex, "Unhandled exception on {Method} {Path}", context.Request.Method, context.Request.Path.Value);

                throw;
            }
        }
    }
}
