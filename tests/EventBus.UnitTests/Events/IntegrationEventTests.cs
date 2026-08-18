using eShop.EventBus.Events;

namespace EventBus.UnitTests.Events;

[TestClass]
public class IntegrationEventTests
{
    [TestMethod]
    public void Default_constructor_assigns_a_non_empty_Id()
    {
        var @event = new IntegrationEvent();

        Assert.AreNotEqual(Guid.Empty, @event.Id);
    }

    [TestMethod]
    public void Default_constructor_assigns_CreationDate_to_the_current_UTC_time()
    {
        var before = DateTime.UtcNow;

        var @event = new IntegrationEvent();

        var after = DateTime.UtcNow;
        Assert.IsTrue(@event.CreationDate >= before && @event.CreationDate <= after);
    }

    [TestMethod]
    public void Object_initializer_values_override_the_constructor_defaults()
    {
        var id = Guid.NewGuid();
        var creationDate = new DateTime(2020, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        var @event = new IntegrationEvent { Id = id, CreationDate = creationDate };

        Assert.AreEqual(id, @event.Id);
        Assert.AreEqual(creationDate, @event.CreationDate);
    }
}
