using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using API.Services;

public class ScheduledHostedService : IHostedService, IDisposable
{
    private readonly IServiceProvider _serviceProvider;
    private System.Timers.Timer _timer;

    public ScheduledHostedService(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _timer = new System.Timers.Timer(1000 * 60 * 60 * 2); // 2 hours in milliseconds
        _timer.Elapsed += DoWork;
        _timer.AutoReset = true;
        _timer.Enabled = true;
        return Task.CompletedTask;
    }

    private void DoWork(object source, ElapsedEventArgs e)
    {
        using (var scope = _serviceProvider.CreateScope())
        {
            var touristGuideService = scope.ServiceProvider.GetRequiredService<WebScrapingService>();
            touristGuideService.GetCitiesData();

            var webScrapingService = scope.ServiceProvider.GetRequiredService<WebScrapingService>();
            webScrapingService.GetCitiesData();
            webScrapingService.ScrapeStations();
        }
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _timer?.Stop();
        return Task.CompletedTask;
    }

    public void Dispose()
    {
        _timer?.Dispose();
    }
}
