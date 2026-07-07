using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace TaskMonitoringApp.Services
{
    public class GuestCleanupService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<GuestCleanupService> _logger;

        public GuestCleanupService(IServiceProvider serviceProvider, ILogger<GuestCleanupService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Guest Cleanup Hosted Service initialized.");

            while (!stoppingToken.IsCancellationRequested)
            {
                var now = DateTime.Now;
                var nextRun = now.Date.AddDays(1); // Next midnight (12:00 AM)
                var delay = nextRun - now;

                _logger.LogInformation("Next guest data reset scheduled at: {NextRun} (in {DelayHours:F2} hours)", nextRun, delay.TotalHours);

                try
                {
                    // Delay execution until midnight
                    await Task.Delay(delay, stoppingToken);

                    _logger.LogInformation("Daily midnight trigger: Resetting guest data...");
                    
                    // Create scoped database work context
                    using (var scope = _serviceProvider.CreateScope())
                    {
                        var seeder = scope.ServiceProvider.GetRequiredService<IGuestSeederService>();
                        await seeder.CleanupAndReseedGuestAsync();
                    }
                }
                catch (TaskCanceledException)
                {
                    // Hosted service stopping
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error occurred during daily guest data reset.");
                    
                    // On error, wait 10 minutes before checking again
                    await Task.Delay(TimeSpan.FromMinutes(10), stoppingToken);
                }
            }
        }
    }
}
