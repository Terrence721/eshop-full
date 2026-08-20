using eShop.IntegrationEventLogEF;
using Microsoft.EntityFrameworkCore;

namespace IntegrationEventLogEF.UnitTests;

[TestClass]
public class IntegrationLogExtensionsTests
{
    private sealed class TestDbContext(DbContextOptions<TestDbContext> options) : DbContext(options)
    {
        protected override void OnModelCreating(ModelBuilder modelBuilder) => modelBuilder.UseIntegrationEventLogs();
    }

    [TestMethod]
    public void UseIntegrationEventLogs_maps_the_entry_to_the_IntegrationEventLog_table()
    {
        using var context = CreateContext();

        var entityType = context.Model.FindEntityType(typeof(IntegrationEventLogEntry))!;

        Assert.AreEqual("IntegrationEventLog", entityType.GetTableName());
    }

    [TestMethod]
    public void UseIntegrationEventLogs_configures_EventId_as_the_sole_primary_key()
    {
        using var context = CreateContext();

        var entityType = context.Model.FindEntityType(typeof(IntegrationEventLogEntry))!;
        var primaryKey = entityType.FindPrimaryKey()!;

        Assert.AreEqual(1, primaryKey.Properties.Count);
        Assert.AreEqual(nameof(IntegrationEventLogEntry.EventId), primaryKey.Properties[0].Name);
    }

    private static TestDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<TestDbContext>()
            .UseNpgsql("Host=localhost;Database=test;Username=test;Password=test")
            .Options;

        return new TestDbContext(options);
    }
}
