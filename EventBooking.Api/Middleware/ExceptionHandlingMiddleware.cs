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
                _logger.LogError(ex, "Unhandled exception: {Message}", ex.Message);
                await HandleAsync(context, ex);
            }
        }

        private static async Task HandleAsync(HttpContext context, Exception exception)
        {
            var (status, message) = exception switch
            {
                KeyNotFoundException      => (StatusCodes.Status404NotFound,                  exception.Message),
                InvalidOperationException => (StatusCodes.Status422UnprocessableEntity,        exception.Message),
                UnauthorizedAccessException => (StatusCodes.Status401Unauthorized,             exception.Message),
                ValidationException ve    => (StatusCodes.Status400BadRequest,
                                              string.Join(" | ", ve.Errors.Select(e => e.ErrorMessage))),
                _                         => (StatusCodes.Status500InternalServerError,        "An unexpected error occurred.")
            };

            context.Response.StatusCode = status;
            context.Response.ContentType = "application/json";

            var body = JsonSerializer.Serialize(new { status, message });
            await context.Response.WriteAsync(body);
        }
    }
}
