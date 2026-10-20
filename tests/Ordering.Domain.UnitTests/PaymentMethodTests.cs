using eShop.Ordering.Domain.AggregatesModel.BuyerAggregate;
using eShop.Ordering.Domain.Exceptions;

namespace eShop.Ordering.Domain.UnitTests;

[TestClass]
public class PaymentMethodTests
{
    private static readonly DateTime FutureExpiration = DateTime.UtcNow.AddYears(1);

    [TestMethod]
    [DataRow(null)]
    [DataRow("")]
    [DataRow("   ")]
    public void Constructor_throws_when_cardNumber_is_null_or_whitespace(string? cardNumber)
    {
        Assert.ThrowsExactly<OrderingDomainException>(
            () => new PaymentMethod(1, "alias", cardNumber!, "123", "Alice", FutureExpiration));
    }

    [TestMethod]
    [DataRow(null)]
    [DataRow("")]
    [DataRow("   ")]
    public void Constructor_throws_when_securityNumber_is_null_or_whitespace(string? securityNumber)
    {
        Assert.ThrowsExactly<OrderingDomainException>(
            () => new PaymentMethod(1, "alias", "4111111111111111", securityNumber!, "Alice", FutureExpiration));
    }

    [TestMethod]
    [DataRow(null)]
    [DataRow("")]
    [DataRow("   ")]
    public void Constructor_throws_when_cardHolderName_is_null_or_whitespace(string? cardHolderName)
    {
        Assert.ThrowsExactly<OrderingDomainException>(
            () => new PaymentMethod(1, "alias", "4111111111111111", "123", cardHolderName!, FutureExpiration));
    }

    [TestMethod]
    public void Constructor_throws_when_expiration_is_in_the_past()
    {
        Assert.ThrowsExactly<OrderingDomainException>(
            () => new PaymentMethod(1, "alias", "4111111111111111", "123", "Alice", DateTime.UtcNow.AddDays(-1)));
    }

    [TestMethod]
    public void IsEqualTo_returns_true_when_cardTypeId_cardNumber_and_expiration_all_match()
    {
        var payment = new PaymentMethod(1, "alias", "4111111111111111", "123", "Alice", FutureExpiration);

        Assert.IsTrue(payment.IsEqualTo(1, "4111111111111111", FutureExpiration));
    }

    [TestMethod]
    public void IsEqualTo_returns_false_when_cardTypeId_differs()
    {
        var payment = new PaymentMethod(1, "alias", "4111111111111111", "123", "Alice", FutureExpiration);

        Assert.IsFalse(payment.IsEqualTo(2, "4111111111111111", FutureExpiration));
    }

    [TestMethod]
    public void IsEqualTo_returns_false_when_cardNumber_differs()
    {
        var payment = new PaymentMethod(1, "alias", "4111111111111111", "123", "Alice", FutureExpiration);

        Assert.IsFalse(payment.IsEqualTo(1, "4222222222222222", FutureExpiration));
    }

    [TestMethod]
    public void IsEqualTo_returns_false_when_expiration_differs()
    {
        var payment = new PaymentMethod(1, "alias", "4111111111111111", "123", "Alice", FutureExpiration);

        Assert.IsFalse(payment.IsEqualTo(1, "4111111111111111", FutureExpiration.AddDays(1)));
    }
}
