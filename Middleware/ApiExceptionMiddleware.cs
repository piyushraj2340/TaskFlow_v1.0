using Microsoft.EntityFrameworkCore;
using TaskMonitoringApp.Exceptions;

namespace TaskMonitoringApp.Middleware
{
    public class ApiExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ApiExceptionMiddleware> _logger;

        public ApiExceptionMiddleware(RequestDelegate next, ILogger<ApiExceptionMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);

                if (context.Response.StatusCode >= 400 && !context.Response.HasStarted)
                {
                    if (string.IsNullOrWhiteSpace(context.Response.ContentType) || !context.Response.ContentType.StartsWith("application/json"))
                    {
                        context.Response.ContentType = "application/json";
                        var response = new
                        {
                            StatusCodes = context.Response.StatusCode,
                            message = GetDefaultMessageForStatusCode(context.Response.StatusCode),
                            Details = (string)null
                        };
                        await context.Response.WriteAsJsonAsync(response);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                await HandleExceptionAsync(context, ex);
            }
        }

        private static string GetDefaultMessageForStatusCode(int statusCode)
        {
            return statusCode switch
            {
                400 => "Bad Request",
                401 => "Unauthorized access",
                403 => "Forbidden access",
                404 => "Resource not found",
                _ => "An error occurred"
            };
        }

        private static Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            var statusCode = exception switch
            {
                NotFoundException => StatusCodes.Status404NotFound,
                ArgumentNullException => StatusCodes.Status400BadRequest,
                ArgumentException => StatusCodes.Status400BadRequest,
                DbUpdateConcurrencyException => StatusCodes.Status500InternalServerError,
                DbUpdateException => StatusCodes.Status500InternalServerError,
                InvalidOperationException => StatusCodes.Status400BadRequest,
                _ => StatusCodes.Status500InternalServerError
            };

            var response = new
            {
                StatusCodes = statusCode,
                message = string.IsNullOrWhiteSpace(exception.Message) ? "Something Went Wrong!" : exception.Message,
                Details = exception.InnerException?.Message
            };

            context.Response.ContentType = "application/json";
            context.Response.StatusCode = statusCode;
            return context.Response.WriteAsJsonAsync(response);
        }
    }
}
