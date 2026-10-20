using eShop.Ordering.API.Application.Commands;
using eShop.Ordering.API.Application.Validations;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace eShop.Ordering.API.UnitTests;

[TestClass]
public class ShipOrderCommandValidatorTests
{
    private static ShipOrderCommandValidator CreateValidator() =>
        new(Substitute.For<ILogger<ShipOrderCommandValidator>>());

    [TestMethod]
    public void Validate_succeeds_when_OrderNumber_is_nonzero()
    {
        var result = CreateValidator().Validate(new ShipOrderCommand(1));

        Assert.IsTrue(result.IsValid);
    }

    [TestMethod]
    public void Validate_fails_when_OrderNumber_is_zero()
    {
        var result = CreateValidator().Validate(new ShipOrderCommand(0));

        Assert.IsFalse(result.IsValid);
    }
}
