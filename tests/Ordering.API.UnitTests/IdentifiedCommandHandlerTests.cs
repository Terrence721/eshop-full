using eShop.Ordering.API.Application.Commands;
using eShop.Ordering.Infrastructure.Idempotency;
using MediatR;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace eShop.Ordering.API.UnitTests;

// IdentifiedCommandHandler<T, R> is abstract; exercised here through one concrete
// subclass (CancelOrderIdentifiedCommandHandler) rather than repeating the same
// base-class behavior for every one of its thin specializations.
[TestClass]
public class IdentifiedCommandHandlerTests
{
    private static CancelOrderIdentifiedCommandHandler CreateHandler(IMediator mediator, IRequestManager requestManager) =>
        new(mediator, requestManager, Substitute.For<ILogger<IdentifiedCommandHandler<CancelOrderCommand, bool>>>());

    [TestMethod]
    public async Task Handle_returns_the_duplicate_result_without_sending_the_inner_command_when_already_seen()
    {
        var requestId = Guid.NewGuid();
        var requestManager = Substitute.For<IRequestManager>();
        requestManager.ExistAsync(requestId).Returns(true);
        var mediator = Substitute.For<IMediator>();
        var handler = CreateHandler(mediator, requestManager);

        var result = await handler.Handle(new IdentifiedCommand<CancelOrderCommand, bool>(new CancelOrderCommand(1), requestId), CancellationToken.None);

        Assert.IsTrue(result);
        await mediator.DidNotReceive().Send(Arg.Any<CancelOrderCommand>(), Arg.Any<CancellationToken>());
        await requestManager.DidNotReceive().CreateRequestForCommandAsync<CancelOrderCommand>(Arg.Any<Guid>());
    }

    [TestMethod]
    public async Task Handle_records_the_request_and_sends_the_inner_command_when_not_a_duplicate()
    {
        var requestId = Guid.NewGuid();
        var requestManager = Substitute.For<IRequestManager>();
        requestManager.ExistAsync(requestId).Returns(false);
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<CancelOrderCommand>(), Arg.Any<CancellationToken>()).Returns(true);
        var handler = CreateHandler(mediator, requestManager);

        var result = await handler.Handle(new IdentifiedCommand<CancelOrderCommand, bool>(new CancelOrderCommand(1), requestId), CancellationToken.None);

        Assert.IsTrue(result);
        await requestManager.Received(1).CreateRequestForCommandAsync<CancelOrderCommand>(requestId);
        await mediator.Received(1).Send(Arg.Is<CancelOrderCommand>(c => c.OrderNumber == 1), Arg.Any<CancellationToken>());
    }

    [TestMethod]
    public async Task Handle_swallows_an_exception_from_the_inner_command_and_returns_the_default_result()
    {
        var requestId = Guid.NewGuid();
        var requestManager = Substitute.For<IRequestManager>();
        requestManager.ExistAsync(requestId).Returns(false);
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<CancelOrderCommand>(), Arg.Any<CancellationToken>()).Returns(Task.FromException<bool>(new InvalidOperationException("boom")));
        var handler = CreateHandler(mediator, requestManager);

        var result = await handler.Handle(new IdentifiedCommand<CancelOrderCommand, bool>(new CancelOrderCommand(1), requestId), CancellationToken.None);

        Assert.IsFalse(result);
    }
}
