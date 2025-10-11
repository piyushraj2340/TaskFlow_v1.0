using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace TaskMonitoringApp.Services;

public class LoggingEmailService : IEmailService
{
    private readonly ILogger<LoggingEmailService> _logger;
    public LoggingEmailService(ILogger<LoggingEmailService> logger) => _logger = logger;

    public Task SendAsync(string to, string subject, string html)
    {
        _logger.LogInformation("SendEmail to={To} subject={Subject} body={Body}", to, subject, html);
        return Task.CompletedTask;
    }
}