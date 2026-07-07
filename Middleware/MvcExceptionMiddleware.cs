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

            // Check if the request expects JSON or is an AJAX call
            var isApiOrAjax = context.Request.Path.StartsWithSegments("/api") ||
                              context.Request.Headers["X-Requested-With"] == "XMLHttpRequest" ||
                              context.Request.Headers["Accept"].ToString().Contains("application/json");

            if (isApiOrAjax)
            {
                context.Response.ContentType = "application/json";
                var response = new
                {
                    StatusCodes = statusCode,
                    message = string.IsNullOrWhiteSpace(exception.Message) ? "Something Went Wrong!" : exception.Message,
                    Details = exception.InnerException?.Message
                };
                return context.Response.WriteAsJsonAsync(response);
            }

            if (_env.IsDevelopment())
            {
                context.Response.ContentType = "text/html";
                return context.Response.WriteAsync($@"
                <!DOCTYPE html>
                <html lang=""en"">
                <head>
                    <meta charset=""UTF-8"">
                    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
                    <title>Application Error</title>
                    <script src=""https://cdn.tailwindcss.com""></script>
                    <link href=""https://cdnjs.cloudflare.com/ajax/libs/font-awesome/6.0.0/css/all.min.css"" rel=""stylesheet"">
                </head>
                <body class=""bg-slate-50 text-slate-800 font-sans min-h-screen flex items-center justify-center p-4 sm:p-6"">
                    <div class=""max-w-4xl w-full bg-white rounded-2xl shadow-xl border border-slate-200 overflow-hidden"">
                        <!-- Red Banner -->
                        <div class=""bg-gradient-to-r from-red-500 to-rose-600 px-6 py-8 text-white flex items-center space-x-4"" style=""background: linear-gradient(135deg, #ef4444 0%, #f43f5e 100%);"">
                            <div class=""bg-white/20 p-4 rounded-full"">
                                <i class=""fas fa-exclamation-triangle text-3xl""></i>
                            </div>
                            <div>
                                <h1 class=""text-2xl sm:text-3xl font-bold tracking-tight"">An Unhandled Exception Occurred</h1>
                                <p class=""text-red-100 mt-1 text-sm sm:text-base font-medium"">HTTP {statusCode} - Internal Server Error (Development Mode)</p>
                            </div>
                        </div>

                        <div class=""p-6 sm:p-8 space-y-6"">
                            <!-- Error Message Card -->
                            <div class=""bg-rose-50 border-l-4 border-red-500 p-4 rounded-r-lg"">
                                <h2 class=""text-red-800 font-semibold text-lg flex items-center"">
                                    <i class=""fas fa-info-circle mr-2""></i> Primary Message
                                </h2>
                                <p class=""text-red-700 mt-1 font-mono text-sm leading-relaxed whitespace-pre-wrap"">
                                    {(string.IsNullOrWhiteSpace(exception.Message) ? "Something Went Wrong!" : exception.Message)}
                                </p>
                            </div>

                            <!-- Inner Exception (if any) -->
                            {(!string.IsNullOrWhiteSpace(exception.InnerException?.Message) ? $@"
                            <div class=""bg-amber-50 border-l-4 border-amber-500 p-4 rounded-r-lg"">
                                <h2 class=""text-amber-800 font-semibold text-md flex items-center"">
                                    <i class=""fas fa-link mr-2""></i> Inner Exception
                                </h2>
                                <p class=""text-amber-700 mt-1 font-mono text-xs leading-relaxed"">
                                    {exception.InnerException.Message}
                                </p>
                            </div>" : "")}

                            <!-- Action Buttons -->
                            <div class=""flex flex-wrap gap-4 pt-2"">
                                <button onclick=""window.location.reload()"" class=""bg-slate-800 hover:bg-slate-900 text-white font-semibold px-5 py-2.5 rounded-lg shadow-sm transition flex items-center cursor-pointer"">
                                    <i class=""fas fa-sync-alt mr-2""></i> Retry Request
                                </button>
                                <a href=""/"" class=""border border-slate-300 hover:bg-slate-50 text-slate-700 font-semibold px-5 py-2.5 rounded-lg transition flex items-center"">
                                    <i class=""fas fa-home mr-2""></i> Go to Dashboard
                                </a>
                            </div>

                            <!-- Collapsible Stack Trace -->
                            <div class=""border border-slate-200 rounded-xl overflow-hidden"">
                                <button onclick=""document.getElementById('stacktrace').classList.toggle('hidden'); document.getElementById('caret').classList.toggle('fa-chevron-down'); document.getElementById('caret').classList.toggle('fa-chevron-up');"" 
                                        class=""w-full text-left px-5 py-4 text-slate-200 font-semibold flex justify-between items-center transition focus:outline-none cursor-pointer"" style=""background: #1e293b; border-bottom: 1px solid #334155;"">
                                    <span class=""flex items-center text-sm""><i class=""fas fa-code mr-2 text-indigo-400""></i> Detailed Stack Trace</span>
                                    <i id=""caret"" class=""fas fa-chevron-down text-slate-400""></i>
                                </button>
                                <div id=""stacktrace"" class=""hidden p-5 overflow-x-auto"" style=""background: #0f172a;"">
                                    <pre class=""text-indigo-200 font-mono text-xs leading-relaxed whitespace-pre-wrap select-all"">{exception.StackTrace}</pre>
                                </div>
                            </div>
                        </div>
                    </div>
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
