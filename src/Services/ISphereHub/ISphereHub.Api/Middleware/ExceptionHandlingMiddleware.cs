using ISphereHub.BuildingBlocks.Exceptions;
using System.Net;
using System.Text.Json;

namespace ISphereHub.Api.Middleware
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
            catch (AppException appEx)
            {
                _logger.LogWarning(appEx, "Handled application exception: {Message}", appEx.Message);
                await WriteErrorResponse(context, appEx.StatusCode, appEx.Message, (appEx as ValidationAppException)?.Errors);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled exception occurred");
                await WriteErrorResponse(context, (int)HttpStatusCode.InternalServerError, "An unexpected error occurred. Please try again later.");
            }
        }

        private static async Task WriteErrorResponse(HttpContext context, int statusCode, string message, IDictionary<string, string[]>? errors = null)
        {
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = statusCode;

            var payload = new
            {
                success = false,
                message,
                errors
            };

            await context.Response.WriteAsync(JsonSerializer.Serialize(payload, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            }));
        }
    }

    public static class ExceptionHandlingMiddlewareExtensions
    {
        public static IApplicationBuilder UseGlobalExceptionHandling(this IApplicationBuilder app)
        {
            return app.UseMiddleware<ExceptionHandlingMiddleware>();
        }
    }
}
