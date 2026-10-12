using eShop.Ordering.API.Application.Commands;
using eShop.Ordering.API.Application.Validations;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace eShop.Ordering.API.UnitTests;

[TestClass]
public class IdentifiedCommandValidatorTests
{
    private static IdentifiedCommandValidator CreateValidator() =>
        new(Substitute.For<ILogger<IdentifiedCommandValidator>>());

    private static CreateOrderCommand CreateInnerCommand() =>
        new([], "user-1", "Alice", "Springfield", "123 Main St", "IL", "USA", "62704",
            "411111111111", "Alice", DateTime.UtcNow.AddYears(1), "123", 1);

    [TestMethod]
    public void Validate_succeeds_when_Id_is_a_real_guid()
    {
        var command = new IdentifiedCommand<CreateOrderCommand, bool>(CreateInnerCommand(), Guid.NewGuid());

        var result = CreateValidator().Validate(command);

        Assert.IsTrue(result.IsValid);
    }

    [TestMethod]
    public void Validate_fails_when_Id_is_empty()
    {
        var command = new IdentifiedCommand<CreateOrderCommand, bool>(CreateInnerCommand(), Guid.Empty);

        var result = CreateValidator().Validate(command);

        Assert.IsFalse(result.IsValid);
    }
}
