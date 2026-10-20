using eShop.Ordering.Domain.AggregatesModel.OrderAggregate;

namespace eShop.Ordering.Domain.UnitTests;

// Address is the simplest real ValueObject subclass in this project, so its
// tests also cover the inherited Equals/GetHashCode/GetCopy contract -
// ValueObject itself is abstract with no concrete behavior of its own to
// test in isolation.
[TestClass]
public class AddressTests
{
    [TestMethod]
    public void Constructor_sets_all_properties()
    {
        var address = new Address("123 Main St", "Springfield", "IL", "USA", "62704");

        Assert.AreEqual("123 Main St", address.Street);
        Assert.AreEqual("Springfield", address.City);
        Assert.AreEqual("IL", address.State);
        Assert.AreEqual("USA", address.Country);
        Assert.AreEqual("62704", address.ZipCode);
    }

    [TestMethod]
    public void Equals_returns_true_for_addresses_with_the_same_values()
    {
        var a = new Address("123 Main St", "Springfield", "IL", "USA", "62704");
        var b = new Address("123 Main St", "Springfield", "IL", "USA", "62704");

        Assert.IsTrue(a.Equals(b));
    }

    [TestMethod]
    public void Equals_returns_false_when_any_component_differs()
    {
        var a = new Address("123 Main St", "Springfield", "IL", "USA", "62704");
        var b = new Address("123 Main St", "Springfield", "IL", "USA", "99999");

        Assert.IsFalse(a.Equals(b));
    }

    [TestMethod]
    public void Equals_returns_false_for_a_different_type()
    {
        var a = new Address("123 Main St", "Springfield", "IL", "USA", "62704");

        Assert.IsFalse(a.Equals("not an address"));
    }

    [TestMethod]
    public void Equals_returns_false_for_null()
    {
        var a = new Address("123 Main St", "Springfield", "IL", "USA", "62704");

        Assert.IsFalse(a.Equals(null));
    }

    [TestMethod]
    public void GetHashCode_matches_for_equal_addresses()
    {
        var a = new Address("123 Main St", "Springfield", "IL", "USA", "62704");
        var b = new Address("123 Main St", "Springfield", "IL", "USA", "62704");

        Assert.AreEqual(a.GetHashCode(), b.GetHashCode());
    }

    [TestMethod]
    public void GetCopy_returns_an_equal_but_distinct_instance()
    {
        var address = new Address("123 Main St", "Springfield", "IL", "USA", "62704");

        var copy = address.GetCopy();

        Assert.AreNotSame(address, copy);
        Assert.IsTrue(address.Equals(copy));
    }
}
