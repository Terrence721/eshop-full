using Microsoft.EntityFrameworkCore;
using eShop.IntegrationEventLogEF.Services;
using eShop.EventBus.Events;

namespace eShop.IntegrationEventLogEF.UnitTests.Services
{
    [TestClass]
    public class IntegrationEventLogServiceTests
    {
        private sealed record TestEvent : IntegrationEvent { public string? Message { get; init; } }

        [TestMethod]
        public void Dispose_does_not_throw()
        {
            var service = new IntegrationEventLogService<TestDbContext>(CreateContext());

            // Should not throw
            service.Dispose();
        }

        [TestMethod]
        public void Dispose_is_idempotent()
        {
            var service = new IntegrationEventLogService<TestDbContext>(CreateContext());

            service.Dispose();
            service.Dispose(); // Should not throw on second call
        }

        private static TestDbContext CreateContext()
        {
            var options = new DbContextOptionsBuilder<TestDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            return new TestDbContext(options);
        }

        private sealed class TestDbContext(DbContextOptions<TestDbContext> options) : DbContext(options)
        {
            public DbSet<IntegrationEventLogEntry> IntegrationEventLog => Set<IntegrationEventLogEntry>();
        }
    }
}
