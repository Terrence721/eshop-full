namespace eShop.IntegrationEventLogEF;

public static class IntegrationLogExtensions
{
    public static void UseIntegrationEventLogs(this ModelBuilder builder)
    {
        builder.Entity<IntegrationEventLogEntry>(entityBuilder =>
        {
            entityBuilder.ToTable("IntegrationEventLog");

            entityBuilder.HasKey(e => e.EventId);
        });
    }
}
