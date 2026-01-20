using Blog.Application.Interfaces;
using Blog.Infrastructure.Options;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace Blog.Infrastructure.Services
{
    internal class CleanupBackgroundService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly TimeSpan _interval;

        public CleanupBackgroundService(IServiceProvider serviceProvider, IOptions<CleanupOptions> options)
        {
            _serviceProvider = serviceProvider;
            var intervalHours = options.Value.IntervalHours;
            _interval = TimeSpan.FromHours(intervalHours);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            // Will run the first time on start
            while (!stoppingToken.IsCancellationRequested)
            {
                using (var scope = _serviceProvider.CreateScope())
                {
                    var cleanupService = scope.ServiceProvider
                        .GetRequiredService<ICleanupService>();

                    await cleanupService.CleanupOrphanedArticleImagesAsync(stoppingToken);
                }

                await Task.Delay(_interval, stoppingToken);
            }
        }
    }
}
