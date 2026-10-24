namespace Webhooks.API.Infrastructure
{
    /// <summary>
    /// EF Core DbContext for Webhooks.API subscriptions.
    /// </summary>
    /// <remarks>
    /// Add migrations using the following command inside the 'Webhooks.API' project directory:
    ///
    /// dotnet ef migrations add [migration-name]
    /// </remarks>
    public class WebhooksContext(DbContextOptions<WebhooksContext> options) : DbContext(options)
    {
        /// <summary>
        /// Webhook subscriptions keyed by user and event type.
        /// </summary>
        public DbSet<WebhookSubscription> Subscriptions { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<WebhookSubscription>(eb =>
            {
                eb.HasIndex(s => s.UserId);
                eb.HasIndex(s => s.Type);
            });
        }
    }
}