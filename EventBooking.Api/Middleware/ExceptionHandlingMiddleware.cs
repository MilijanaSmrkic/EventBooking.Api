using FluentValidation;
using System.Text.Json;

namespace EventBooking.Api.Middleware
{
    public class ExceptionHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionHandlingMiddleware> _logger;

        public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                await HandleAsync(context, ex);
            }
        }

        private async Task HandleAsync(HttpContext context, Exception exception)
        {
            var (status, message) = exception switch
            {
                KeyNotFoundException        => (StatusCodes.Status404NotFound,               exception.Message),
                InvalidOperationException   => (StatusCodes.Status422UnprocessableEntity,    exception.Message),
                UnauthorizedAccessException => (StatusCodes.Status401Unauthorized,           exception.Message),
                ValidationException ve      => (StatusCodes.Status400BadRequest,
                                                string.Join(" | ", ve.Errors.Select(e => e.ErrorMessage))),
                _                           => (StatusCodes.Status500InternalServerError,    "An unexpected error occurred.")
            };

            if (status >= 500)
                _logger.LogError(exception,
                    "Unhandled server error on {Method} {Path}: {Message}",
                    context.Request.Method, context.Request.Path, exception.Message);
            else
                _logger.LogWarning(
                    "Client error {StatusCode} on {Method} {Path}: {Message}",
                    status, context.Request.Method, context.Request.Path, exception.Message);

            context.Response.StatusCode = status;
            context.Response.ContentType = "application/json";

            var body = JsonSerializer.Serialize(new { status, message });
            await context.Response.WriteAsync(body);
        }
    }
}
