using eShop.Ordering.API.Application.Behaviors;
using eShop.Ordering.API.Application.Commands;
using eShop.Ordering.Domain.Exceptions;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace eShop.Ordering.API.UnitTests;

[TestClass]
public class ValidatorBehaviorTests
{
    private static ValidatorBehavior<CancelOrderCommand, bool> CreateBehavior(params IValidator<CancelOrderCommand>[] validators) =>
        new(validators, Substitute.For<ILogger<ValidatorBehavior<CancelOrderCommand, bool>>>());

    [TestMethod]
    public async Task Handle_calls_next_when_there_are_no_validators()
    {
        var behavior = CreateBehavior();

        var result = await behavior.Handle(new CancelOrderCommand(1), ct => Task.FromResult(true), CancellationToken.None);

        Assert.IsTrue(result);
    }

    [TestMethod]
    public async Task Handle_calls_next_when_every_validator_passes()
    {
        var validator = Substitute.For<IValidator<CancelOrderCommand>>();
        validator.ValidateAsync(Arg.Any<CancelOrderCommand>(), Arg.Any<CancellationToken>())
            .Returns(new ValidationResult());
        var behavior = CreateBehavior(validator);

        var result = await behavior.Handle(new CancelOrderCommand(1), ct => Task.FromResult(true), CancellationToken.None);

        Assert.IsTrue(result);
    }

    [TestMethod]
    public async Task Handle_throws_when_a_validator_reports_failures()
    {
        var validator = Substitute.For<IValidator<CancelOrderCommand>>();
        validator.ValidateAsync(Arg.Any<CancelOrderCommand>(), Arg.Any<CancellationToken>())
            .Returns(new ValidationResult([new ValidationFailure("OrderNumber", "No orderId found")]));
        var behavior = CreateBehavior(validator);

        await Assert.ThrowsExactlyAsync<OrderingDomainException>(
            () => behavior.Handle(new CancelOrderCommand(1), ct => Task.FromResult(true), CancellationToken.None));
    }

    [TestMethod]
    public async Task Handle_does_not_call_next_when_validation_fails()
    {
        var validator = Substitute.For<IValidator<CancelOrderCommand>>();
        validator.ValidateAsync(Arg.Any<CancelOrderCommand>(), Arg.Any<CancellationToken>())
            .Returns(new ValidationResult([new ValidationFailure("OrderNumber", "No orderId found")]));
        var behavior = CreateBehavior(validator);
        var nextCalled = false;

        try
        {
            await behavior.Handle(new CancelOrderCommand(1), ct => { nextCalled = true; return Task.FromResult(true); }, CancellationToken.None);
        }
        catch (OrderingDomainException)
        {
        }

        Assert.IsFalse(nextCalled);
    }
}
