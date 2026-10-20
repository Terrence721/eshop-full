using eShop.Ordering.Domain.AggregatesModel.BuyerAggregate;
using eShop.Ordering.Domain.Events;

namespace eShop.Ordering.Domain.UnitTests;

[TestClass]
public class BuyerTests
{
    private static readonly DateTime FutureExpiration = DateTime.UtcNow.AddYears(1);

    [TestMethod]
    public void Constructor_sets_IdentityGuid_and_Name()
    {
        var buyer = new Buyer("identity-1", "Alice");

        Assert.AreEqual("identity-1", buyer.IdentityGuid);
        Assert.AreEqual("Alice", buyer.Name);
    }

    [TestMethod]
    [DataRow(null)]
    [DataRow("")]
    [DataRow("   ")]
    public void Constructor_throws_when_identity_is_null_or_whitespace(string? identity)
    {
        Assert.ThrowsExactly<ArgumentNullException>(() => new Buyer(identity!, "Alice"));
    }

    [TestMethod]
    [DataRow(null)]
    [DataRow("")]
    [DataRow("   ")]
    public void Constructor_throws_when_name_is_null_or_whitespace(string? name)
    {
        Assert.ThrowsExactly<ArgumentNullException>(() => new Buyer("identity-1", name!));
    }

    [TestMethod]
    public void PaymentMethods_starts_empty()
    {
        var buyer = new Buyer("identity-1", "Alice");

        Assert.IsEmpty(buyer.PaymentMethods);
    }

    [TestMethod]
    public void VerifyOrAddPaymentMethod_adds_a_new_payment_method_when_none_matches()
    {
        var buyer = new Buyer("identity-1", "Alice");

        var payment = buyer.VerifyOrAddPaymentMethod(
            1, "alias", "4111111111111111", "123", "Alice", FutureExpiration, orderId: 100);

        Assert.HasCount(1, buyer.PaymentMethods.ToList());
        CollectionAssert.Contains(buyer.PaymentMethods.ToList(), payment);
    }

    [TestMethod]
    public void VerifyOrAddPaymentMethod_returns_the_existing_payment_method_when_one_matches()
    {
        var buyer = new Buyer("identity-1", "Alice");
        var first = buyer.VerifyOrAddPaymentMethod(
            1, "alias", "4111111111111111", "123", "Alice", FutureExpiration, orderId: 100);

        var second = buyer.VerifyOrAddPaymentMethod(
            1, "a different alias", "4111111111111111", "123", "Alice", FutureExpiration, orderId: 200);

        Assert.AreSame(first, second);
        Assert.HasCount(1, buyer.PaymentMethods.ToList());
    }

    [TestMethod]
    public void VerifyOrAddPaymentMethod_raises_a_domain_event_for_a_new_payment()
    {
        var buyer = new Buyer("identity-1", "Alice");

        var payment = buyer.VerifyOrAddPaymentMethod(
            1, "alias", "4111111111111111", "123", "Alice", FutureExpiration, orderId: 100);

        var raised = buyer.DomainEvents!.OfType<BuyerAndPaymentMethodVerifiedDomainEvent>().Single();
        Assert.AreSame(buyer, raised.Buyer);
        Assert.AreSame(payment, raised.Payment);
        Assert.AreEqual(100, raised.OrderId);
    }

    [TestMethod]
    public void VerifyOrAddPaymentMethod_raises_a_domain_event_for_an_existing_payment_too()
    {
        var buyer = new Buyer("identity-1", "Alice");
        buyer.VerifyOrAddPaymentMethod(1, "alias", "4111111111111111", "123", "Alice", FutureExpiration, orderId: 100);

        buyer.VerifyOrAddPaymentMethod(1, "alias", "4111111111111111", "123", "Alice", FutureExpiration, orderId: 200);

        var events = buyer.DomainEvents!.OfType<BuyerAndPaymentMethodVerifiedDomainEvent>().ToList();
        Assert.HasCount(2, events);
        Assert.AreEqual(200, events[1].OrderId);
    }
}
