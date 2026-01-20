using Blog.Application.Interfaces;
using Blog.Infrastructure.Options;
using Blog.Infrastructure.Persistence.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Blog.Infrastructure.Services
{
    internal class CleanupService : ICleanupService
    {
        private readonly BlogDbContext _context;

        private readonly IFileStorageService _fileStorageService;
        private readonly ILogger<ICleanupService> _logger;
        private readonly CleanupOptions _options;

        public CleanupService(
            BlogDbContext context,
            IFileStorageService fileStorage,
            ILogger<ICleanupService> logger,
            IOptions<CleanupOptions> options)
        {
            _context = context;
            _fileStorageService = fileStorage;
            _logger = logger;
            _options = options.Value;
        }

        public async Task CleanupOrphanedArticleImagesAsync(CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Starting orphaned article image cleanup job");

            var cutoffDate = DateTime.UtcNow.AddDays(-_options.OrphanArticleImageRetentionDays);

            var orphanedArticleImages = await _context.ArticleImages
                .Where(articleImage => articleImage.ArticleId == null && articleImage.CreatedAt < cutoffDate)
                .ToListAsync(cancellationToken);

            if (!orphanedArticleImages.Any())
            {
                _logger.LogInformation("No orphaned article images have been found");
                return;
            }

            _logger.LogInformation($"Found {orphanedArticleImages.Count} orphaned images to clean up");

            foreach (var articleImage in orphanedArticleImages)
            {
                try
                {
                    var deleteResult = await _fileStorageService.DeleteFileAsync(articleImage.FileName);

                    if (deleteResult.IsSuccess)
                    {
                        _context.ArticleImages.Remove(articleImage);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError( ex, $"Failed to delete orphaned image: {articleImage.Id} - {articleImage.FileName}");
                }
            }

            try
            {
                await _context.SaveChangesAsync(cancellationToken);
                _logger.LogInformation("Cleanup completed.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to save cleanup changes to database.");
                throw;
            }
        }
    }
}
