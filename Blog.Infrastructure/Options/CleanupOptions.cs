namespace Blog.Infrastructure.Options
{
    public class CleanupOptions
    {
        public int OrphanArticleImageRetentionDays { get; set; } = 7;
        public int IntervalHours { get; set; } = 24;
    }
}
