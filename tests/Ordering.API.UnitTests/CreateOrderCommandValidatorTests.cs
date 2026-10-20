using eShop.Ordering.API.Application.Commands;
using eShop.Ordering.API.Application.Models;
using eShop.Ordering.API.Application.Validations;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace eShop.Ordering.API.UnitTests;

[TestClass]
public class CreateOrderCommandValidatorTests
{
    private static readonly List<BasketItem> ValidItems =
    [
        new() { ProductId = 1, ProductName = "Widget", PictureUrl = "widget.png", UnitPrice = 10.00m, Quantity = 1 },
    ];

    private static CreateOrderCommandValidator CreateValidator() =>
        new(Substitute.For<ILogger<CreateOrderCommandValidator>>());

    private static CreateOrderCommand CreateValidCommand(
        string city = "Springfield", string cardNumber = "411111111111", string cardSecurityNumber = "123",
        DateTime? cardExpiration = null, int cardTypeId = 1, List<BasketItem>? items = null) =>
        new(items ?? ValidItems, "user-1", "Alice", city, "123 Main St", "IL", "USA", "62704",
            cardNumber, "Alice", cardExpiration ?? DateTime.UtcNow.AddYears(1), cardSecurityNumber, cardTypeId);

    [TestMethod]
    public void Validate_succeeds_for_a_valid_command()
    {
        var result = CreateValidator().Validate(CreateValidCommand());

        Assert.IsTrue(result.IsValid);
    }

    [TestMethod]
    public void Validate_fails_when_City_is_empty()
    {
        var result = CreateValidator().Validate(CreateValidCommand(city: ""));

        Assert.IsFalse(result.IsValid);
    }

    [TestMethod]
    public void Validate_fails_when_CardNumber_is_empty()
    {
        var result = CreateValidator().Validate(CreateValidCommand(cardNumber: ""));

        Assert.IsFalse(result.IsValid);
    }

    [TestMethod]
    public void Validate_fails_when_CardNumber_is_shorter_than_12_characters()
    {
        var result = CreateValidator().Validate(CreateValidCommand(cardNumber: "1234567890"));

        Assert.IsFalse(result.IsValid);
    }

    [TestMethod]
    public void Validate_fails_when_CardNumber_is_longer_than_19_characters()
    {
        var result = CreateValidator().Validate(CreateValidCommand(cardNumber: new string('1', 20)));

        Assert.IsFalse(result.IsValid);
    }

    [TestMethod]
    public void Validate_succeeds_when_CardNumber_is_exactly_12_characters()
    {
        var result = CreateValidator().Validate(CreateValidCommand(cardNumber: new string('1', 12)));

        Assert.IsTrue(result.IsValid);
    }

    [TestMethod]
    public void Validate_fails_when_CardExpiration_is_in_the_past()
    {
        var result = CreateValidator().Validate(CreateValidCommand(cardExpiration: DateTime.UtcNow.AddDays(-1)));

        Assert.IsFalse(result.IsValid);
    }

    [TestMethod]
    public void Validate_succeeds_when_CardExpiration_is_in_the_future()
    {
        var result = CreateValidator().Validate(CreateValidCommand(cardExpiration: DateTime.UtcNow.AddDays(1)));

        Assert.IsTrue(result.IsValid);
    }

    [TestMethod]
    public void Validate_fails_when_CardSecurityNumber_is_not_exactly_3_characters()
    {
        var result = CreateValidator().Validate(CreateValidCommand(cardSecurityNumber: "12"));

        Assert.IsFalse(result.IsValid);
    }

    [TestMethod]
    public void Validate_fails_when_CardTypeId_is_zero()
    {
        var result = CreateValidator().Validate(CreateValidCommand(cardTypeId: 0));

        Assert.IsFalse(result.IsValid);
    }

    [TestMethod]
    public void Validate_fails_when_OrderItems_is_empty()
    {
        var result = CreateValidator().Validate(CreateValidCommand(items: []));

        Assert.IsFalse(result.IsValid);
    }
}
