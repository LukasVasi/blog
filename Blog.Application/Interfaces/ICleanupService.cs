namespace Blog.Application.Interfaces
{
    public interface ICleanupService
    {
        /// <summary>
        /// Cleans up the orphaned article images by removing the domain entities and the
        /// associated stored files. Orphaned article images are images without a set
        /// article which means they are not used by any article.
        /// </summary>
        /// <param name="cancellationToken">
        /// The cancellation token.
        /// </param>
        public Task CleanupOrphanedArticleImagesAsync(CancellationToken cancellationToken);
    }
}
