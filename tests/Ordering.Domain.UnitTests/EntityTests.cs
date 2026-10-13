using eShop.Ordering.Domain.SeedWork;
using MediatR;

namespace eShop.Ordering.Domain.UnitTests;

file sealed class TestEntity : Entity
{
    public void SetId(int id) => Id = id;
}

file sealed record TestNotification : INotification;

[TestClass]
public class EntityTests
{
    [TestMethod]
    public void IsTransient_is_true_for_a_default_id()
    {
        var entity = new TestEntity();

        Assert.IsTrue(entity.IsTransient());
    }

    [TestMethod]
    public void IsTransient_is_false_once_an_id_is_set()
    {
        var entity = new TestEntity();
        entity.SetId(1);

        Assert.IsFalse(entity.IsTransient());
    }

    [TestMethod]
    public void Equals_returns_false_for_two_transient_entities_even_with_the_same_id()
    {
        var a = new TestEntity();
        var b = new TestEntity();

        Assert.IsFalse(a.Equals(b));
    }

    [TestMethod]
    public void Equals_returns_true_for_entities_with_the_same_id()
    {
        var a = new TestEntity();
        a.SetId(1);
        var b = new TestEntity();
        b.SetId(1);

        Assert.IsTrue(a.Equals(b));
    }

    [TestMethod]
    public void Equals_returns_false_for_entities_with_different_ids()
    {
        var a = new TestEntity();
        a.SetId(1);
        var b = new TestEntity();
        b.SetId(2);

        Assert.IsFalse(a.Equals(b));
    }

    [TestMethod]
    public void Equals_returns_false_for_a_non_entity_object()
    {
        var a = new TestEntity();
        a.SetId(1);

        Assert.IsFalse(a.Equals("not an entity"));
    }

    [TestMethod]
    public void Equality_operator_matches_Equals()
    {
        var a = new TestEntity();
        a.SetId(1);
        var b = new TestEntity();
        b.SetId(1);

        Assert.IsTrue(a == b);
        Assert.IsFalse(a != b);
    }

    [TestMethod]
    public void Equality_operator_treats_two_nulls_as_equal()
    {
        TestEntity? a = null;
        TestEntity? b = null;

        Assert.IsTrue(a == b);
    }

    [TestMethod]
    public void GetHashCode_is_stable_for_a_non_transient_entity()
    {
        var entity = new TestEntity();
        entity.SetId(1);

        Assert.AreEqual(entity.GetHashCode(), entity.GetHashCode());
    }

    [TestMethod]
    public void DomainEvents_is_null_until_one_is_added()
    {
        var entity = new TestEntity();

        Assert.IsNull(entity.DomainEvents);
    }

    [TestMethod]
    public void AddDomainEvent_makes_DomainEvents_contain_the_event()
    {
        var entity = new TestEntity();
        var notification = new TestNotification();

        entity.AddDomainEvent(notification);

        CollectionAssert.Contains(entity.DomainEvents!.ToList(), notification);
    }

    [TestMethod]
    public void RemoveDomainEvent_removes_a_previously_added_event()
    {
        var entity = new TestEntity();
        var notification = new TestNotification();
        entity.AddDomainEvent(notification);

        entity.RemoveDomainEvent(notification);

        Assert.IsEmpty(entity.DomainEvents!);
    }

    [TestMethod]
    public void ClearDomainEvents_empties_the_collection()
    {
        var entity = new TestEntity();
        entity.AddDomainEvent(new TestNotification());
        entity.AddDomainEvent(new TestNotification());

        entity.ClearDomainEvents();

        Assert.IsEmpty(entity.DomainEvents!);
    }
}
