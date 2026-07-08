using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using TaskMonitoringApp.Models.Data;

namespace TaskMonitoringApp.Services
{
    public class DbKeepAliveService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<DbKeepAliveService> _logger;
        private readonly IConfiguration _configuration;

        public DbKeepAliveService(IServiceProvider serviceProvider, ILogger<DbKeepAliveService> logger, IConfiguration configuration)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
            _configuration = configuration;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Database Keep-Alive Hosted Service initialized.");

            var enableKeepAlive = _configuration.GetValue<bool>("BackgroundServices:EnableDbKeepAlive");
            if (!enableKeepAlive)
            {
                _logger.LogInformation("Database Keep-Alive Hosted Service is disabled in configuration.");
                return;
            }

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using (var scope = _serviceProvider.CreateScope())
                    {
                        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                        
                        // Execute a very lightweight raw SQL query to keep the serverless database active
                        await dbContext.Database.ExecuteSqlRawAsync("SELECT 1", stoppingToken);
                        
                        _logger.LogDebug("Database keep-alive ping sent successfully.");
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning("Failed to execute database keep-alive ping: {Message}", ex.Message);
                }

                // Wait for 5 minutes before pinging again
                await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
            }
        }
    }
}
