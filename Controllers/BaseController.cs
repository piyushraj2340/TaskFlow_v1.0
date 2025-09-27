using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Serilog;
using System.Diagnostics;

namespace TaskMonitoringApp.Controllers
{
    public abstract class BaseController : Controller
    {
        protected readonly ILogger<BaseController> _logger;
        protected readonly string _controllerName;
        protected readonly Stopwatch _stopwatch;

        protected BaseController(ILogger<BaseController> logger)
        {
            _logger = logger;
            _controllerName = GetType().Name;
            _stopwatch = new Stopwatch();
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            _stopwatch.Start();
            var actionName = context.ActionDescriptor.DisplayName;
            var parameters = string.Join(", ", context.ActionArguments.Select(x => $"{x.Key}={x.Value}"));
            
            _logger.LogInformation(
                "Starting {Controller}.{Action} with parameters: {Parameters}",
                _controllerName,
                actionName,
                parameters);

            base.OnActionExecuting(context);
        }

        public override void OnActionExecuted(ActionExecutedContext context)
        {
            _stopwatch.Stop();
            var actionName = context.ActionDescriptor.DisplayName;
            
            if (context.Exception != null)
            {
                _logger.LogError(
                    context.Exception,
                    "Error in {Controller}.{Action}: {ErrorMessage}",
                    _controllerName,
                    actionName,
                    context.Exception.Message);
            }
            else
            {
                _logger.LogInformation(
                    "Completed {Controller}.{Action} in {ElapsedMilliseconds}ms",
                    _controllerName,
                    actionName,
                    _stopwatch.ElapsedMilliseconds);
            }

            base.OnActionExecuted(context);
        }

        protected IActionResult LoggedNotFound(string message = null)
        {
            _logger.LogWarning("NotFound: {Message}", message ?? "Resource not found");
            return NotFound(message);
        }

        protected IActionResult LoggedBadRequest(string message = null)
        {
            _logger.LogWarning("BadRequest: {Message}", message ?? "Invalid request");
            return BadRequest(message);
        }

        protected IActionResult LoggedUnauthorized(string message = null)
        {
            _logger.LogWarning("Unauthorized: {Message}", message ?? "Unauthorized access");
            return Unauthorized(message);
        }

        protected void LogError(Exception ex, string message = null)
        {
            _logger.LogError(ex, "{Controller}: {Message}", _controllerName, message ?? ex.Message);
        }

        protected void LogInformation(string message)
        {
            _logger.LogInformation("{Controller}: {Message}", _controllerName, message);
        }

        protected void LogWarning(string message)
        {
            _logger.LogWarning("{Controller}: {Message}", _controllerName, message);
        }
    }
}