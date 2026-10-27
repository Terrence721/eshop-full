using eShop.Ordering.API.Application.Commands;
using eShop.Ordering.API.Application.Models;
using eShop.Ordering.API.Application.Validations;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace eShop.Ordering.API.UnitTests
{
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
            string userId = "user-1", string userName = "Alice", string city = "Springfield",
            string street = "123 Main St", string state = "IL", string country = "USA",
            string zipCode = "62704", string cardNumber = "411111111111",
            string cardHolderName = "Alice", DateTime? cardExpiration = null,
            string cardSecurityNumber = "123", int cardTypeId = 1,
            List<BasketItem>? items = null) =>
            new(items ?? ValidItems, userId, userName, city, street, state, country, zipCode,
                cardNumber, cardHolderName, cardExpiration ?? DateTime.UtcNow.AddYears(1), cardSecurityNumber, cardTypeId);

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

    [TestMethod]
    public void Validate_fails_when_UserId_is_empty()
    {
        Assert.IsFalse(CreateValidator().Validate(CreateValidCommand(userId: "")).IsValid);
    }

    [TestMethod]
    public void Validate_fails_when_UserName_is_empty()
    {
        Assert.IsFalse(CreateValidator().Validate(CreateValidCommand(userName: "")).IsValid);
    }

    [TestMethod]
    public void Validate_fails_when_Street_is_empty()
    {
        Assert.IsFalse(CreateValidator().Validate(CreateValidCommand(street: "")).IsValid);
    }

    [TestMethod]
    public void Validate_fails_when_State_is_empty()
    {
        Assert.IsFalse(CreateValidator().Validate(CreateValidCommand(state: "")).IsValid);
    }

    [TestMethod]
    public void Validate_fails_when_Country_is_empty()
    {
        Assert.IsFalse(CreateValidator().Validate(CreateValidCommand(country: "")).IsValid);
    }

    [TestMethod]
    public void Validate_fails_when_ZipCode_is_empty()
    {
        Assert.IsFalse(CreateValidator().Validate(CreateValidCommand(zipCode: "")).IsValid);
    }

    [TestMethod]
    public void Validate_fails_when_CardHolderName_is_empty()
    {
        Assert.IsFalse(CreateValidator().Validate(CreateValidCommand(cardHolderName: "")).IsValid);
    }

    [TestMethod]
    public void Validate_fails_when_CardExpiration_is_in_the_past_or_default()
    {
        Assert.IsFalse(CreateValidator().Validate(CreateValidCommand(cardExpiration: DateTime.MinValue)).IsValid);
    }

    [TestMethod]
    public void Validate_fails_when_CardSecurityNumber_is_longer_than_3_characters()
    {
        Assert.IsFalse(CreateValidator().Validate(CreateValidCommand(cardSecurityNumber: "1234")).IsValid);
    }

    [TestMethod]
    public void Validate_succeeds_when_logger_trace_is_enabled()
    {
        var logger = Substitute.For<ILogger<CreateOrderCommandValidator>>();
        logger.IsEnabled(LogLevel.Trace).Returns(true);
        var command = CreateValidCommand();

        var result = new CreateOrderCommandValidator(logger).Validate(command);

        Assert.IsTrue(result.IsValid);
    }

    }
}
