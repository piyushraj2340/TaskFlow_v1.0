using System.Threading.Tasks;

namespace TaskMonitoringApp.Services;

public interface IEmailService
{
    Task SendAsync(string to, string subject, string html);
}