using JardinConecta.Exceptions;
using Microsoft.AspNetCore.Mvc;
using Serilog.Context;

namespace JardinConecta.Middleware;

public class ErrorHandlingMiddleware
{
    private readonly RequestDelegate _next;

    public ErrorHandlingMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, IProblemDetailsService problemDetailsService)
    {
        using (LogContext.PushProperty("TraceId", context.TraceIdentifier))
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                if (MustLogException(ex))
                    Serilog.Log.Logger
                        .ForContext("TraceId", context.TraceIdentifier)
                        .ForContext("Method",  context.Request.Method)
                        .ForContext("Path",    context.Request.Path.Value)
                        .Error(ex, "Unhandled exception on {Method} {Path}", context.Request.Method, context.Request.Path.Value);

                await HandleExceptionAsync(context, ex, problemDetailsService);
            }
        }
    }

    private static async Task HandleExceptionAsync(
        HttpContext context,
        Exception ex,
        IProblemDetailsService problemDetailsService)
    {
        var (status, title, detail) = MapException(ex);

        var problem = new ProblemDetails
        {
            Status = status,
            Title = title,
            Detail = detail
        };

        context.Response.StatusCode = status;

        await problemDetailsService.WriteAsync(new ProblemDetailsContext
        {
            HttpContext = context,
            ProblemDetails = problem
        });
    }

    private static bool MustLogException(Exception ex) => ex switch
    {
        ArgumentException => false,
        InvalidOperationException => false,
        _ => true
    };

    private static (int status, string title, string? detail) MapException(Exception ex) => ex switch
    {
        ExternalServiceException => (
            StatusCodes.Status502BadGateway,
            "External service error",
            null
        ),

        ArgumentException e => (
            StatusCodes.Status400BadRequest,
            "Bad request",
            e.Message
        ),

        UnauthorizedAccessException e => (
            StatusCodes.Status403Forbidden,
            "Forbidden",
            e.Message
        ),

        InvalidOperationException e => (
            StatusCodes.Status409Conflict,
            "Invalid operation",
            e.Message
        ),

        KeyNotFoundException e => (
            StatusCodes.Status404NotFound,
            "Resource not found",
            e.Message
        ),

        _ => (
            StatusCodes.Status500InternalServerError,
            "Internal server error",
            ex.Message
        )
    };
}
