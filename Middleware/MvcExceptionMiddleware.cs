using Microsoft.EntityFrameworkCore;
using TaskMonitoringApp.Exceptions;

namespace TaskMonitoringApp.Middleware
{
    public class MvcExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<MvcExceptionMiddleware> _logger;
        private readonly IWebHostEnvironment _env;

        public MvcExceptionMiddleware(RequestDelegate next, ILogger<MvcExceptionMiddleware> logger, IWebHostEnvironment env)
        {
            _next = next;
            _logger = logger;
            _env = env;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                await HandleExceptionAsync(context, ex);
            }
        }

        private Task HandleExceptionAsync(HttpContext context, Exception exception)
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

            context.Response.StatusCode = statusCode;

            if (_env.IsDevelopment())
            {
                // Show detailed error page in development
                return context.Response.WriteAsync($@"
                <html>
                    <body>
                        <h1>Error: {(string.IsNullOrWhiteSpace(exception.Message) ? "Something Went Wrong!" : exception.Message)}</h1>
                        <p>{exception.StackTrace}</p>
                        <p>{exception.InnerException?.Message}</p>
                        <p>{(string.IsNullOrWhiteSpace(exception.InnerException?.Message) ? null : exception.InnerException?.Message)}</p>
                    </body>
                </html>");
            }
            else
            {
                // Redirect to a user-friendly error page in production
                context.Response.Redirect($"/Error?statusCode={statusCode}&message={exception.Message}");
                return Task.CompletedTask;
            }
        }
    }
}
