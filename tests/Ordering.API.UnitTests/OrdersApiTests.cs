using eShop.Ordering.API.Application.Commands;
using eShop.Ordering.API.Application.Models;
using eShop.Ordering.API.Application.Queries;
using eShop.Ordering.API.Infrastructure.Services;
using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace eShop.Ordering.API.UnitTests;

[TestClass]
public class OrdersApiTests
{
    private static OrderServices CreateServices(IMediator? mediator = null, IOrderQueries? queries = null, IIdentityService? identityService = null) =>
        new(mediator ?? Substitute.For<IMediator>(), queries ?? Substitute.For<IOrderQueries>(),
            identityService ?? Substitute.For<IIdentityService>(), Substitute.For<ILogger<OrderServices>>());

    private static CreateOrderRequest CreateOrderRequest(string cardNumber = "4111111111111111") =>
        new("user-1", "Alice", "Springfield", "123 Main St", "IL", "USA", "62704",
            cardNumber, "Alice", DateTime.UtcNow.AddYears(1), "123", 1, "buyer-1", []);

    [TestMethod]
    public async Task CancelOrderAsync_returns_bad_request_for_an_empty_request_id()
    {
        var result = await OrdersApi.CancelOrderAsync(Guid.Empty, new CancelOrderCommand(1), CreateServices());

        Assert.IsInstanceOfType<BadRequest<string>>(result.Result);
    }

    [TestMethod]
    public async Task CancelOrderAsync_returns_ok_when_the_command_succeeds()
    {
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<IdentifiedCommand<CancelOrderCommand, bool>>()).Returns(true);

        var result = await OrdersApi.CancelOrderAsync(Guid.NewGuid(), new CancelOrderCommand(1), CreateServices(mediator));

        Assert.IsInstanceOfType<Ok>(result.Result);
    }

    [TestMethod]
    public async Task CancelOrderAsync_returns_a_problem_when_the_command_fails()
    {
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<IdentifiedCommand<CancelOrderCommand, bool>>()).Returns(false);

        var result = await OrdersApi.CancelOrderAsync(Guid.NewGuid(), new CancelOrderCommand(1), CreateServices(mediator));

        Assert.IsInstanceOfType<ProblemHttpResult>(result.Result);
    }

    [TestMethod]
    public async Task ShipOrderAsync_returns_bad_request_for_an_empty_request_id()
    {
        var result = await OrdersApi.ShipOrderAsync(Guid.Empty, new ShipOrderCommand(1), CreateServices());

        Assert.IsInstanceOfType<BadRequest<string>>(result.Result);
    }

    [TestMethod]
    public async Task ShipOrderAsync_returns_ok_when_the_command_succeeds()
    {
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<IdentifiedCommand<ShipOrderCommand, bool>>()).Returns(true);

        var result = await OrdersApi.ShipOrderAsync(Guid.NewGuid(), new ShipOrderCommand(1), CreateServices(mediator));

        Assert.IsInstanceOfType<Ok>(result.Result);
    }

    [TestMethod]
    public async Task ShipOrderAsync_returns_a_problem_when_the_command_fails()
    {
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<IdentifiedCommand<ShipOrderCommand, bool>>()).Returns(false);

        var result = await OrdersApi.ShipOrderAsync(Guid.NewGuid(), new ShipOrderCommand(1), CreateServices(mediator));

        Assert.IsInstanceOfType<ProblemHttpResult>(result.Result);
    }

    [TestMethod]
    public async Task GetOrderAsync_returns_the_order_when_found()
    {
        var queries = Substitute.For<IOrderQueries>();
        queries.GetOrderAsync(5).Returns(new Order { OrderNumber = 5 });

        var result = await OrdersApi.GetOrderAsync(5, CreateServices(queries: queries));

        var ok = (Ok<Order>)result.Result;
        Assert.AreEqual(5, ok.Value!.OrderNumber);
    }

    [TestMethod]
    public async Task GetOrderAsync_returns_not_found_when_the_query_throws()
    {
        var queries = Substitute.For<IOrderQueries>();
        queries.GetOrderAsync(999).Returns(Task.FromException<Order>(new KeyNotFoundException()));

        var result = await OrdersApi.GetOrderAsync(999, CreateServices(queries: queries));

        Assert.IsInstanceOfType<NotFound>(result.Result);
    }

    [TestMethod]
    public async Task GetOrdersByUserAsync_returns_the_orders_for_the_current_identity()
    {
        var identityService = Substitute.For<IIdentityService>();
        identityService.GetUserIdentity().Returns("guid-1");
        var queries = Substitute.For<IOrderQueries>();
        queries.GetOrdersFromUserAsync("guid-1").Returns([new OrderSummary { OrderNumber = 1 }]);

        var result = await OrdersApi.GetOrdersByUserAsync(CreateServices(queries: queries, identityService: identityService));

        Assert.AreEqual(1, result.Value!.Single().OrderNumber);
    }

    [TestMethod]
    public async Task GetCardTypesAsync_returns_the_queried_card_types()
    {
        var queries = Substitute.For<IOrderQueries>();
        queries.GetCardTypesAsync().Returns([new CardType { Id = 1, Name = "Amex" }]);

        var result = await OrdersApi.GetCardTypesAsync(queries);

        Assert.AreEqual("Amex", result.Value!.Single().Name);
    }

    [TestMethod]
    public async Task CreateOrderDraftAsync_returns_the_drafted_order_from_the_mediator()
    {
        var expected = new OrderDraftDTO { OrderItems = [], Total = 42m };
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<CreateOrderDraftCommand>()).Returns(expected);

        var result = await OrdersApi.CreateOrderDraftAsync(new CreateOrderDraftCommand("buyer-1", []), CreateServices(mediator));

        Assert.AreEqual(42m, result.Total);
    }

    [TestMethod]
    public async Task CreateOrderAsync_returns_bad_request_for_an_empty_request_id()
    {
        var result = await OrdersApi.CreateOrderAsync(Guid.Empty, CreateOrderRequest(), CreateServices());

        Assert.IsInstanceOfType<BadRequest<string>>(result.Result);
    }

    [TestMethod]
    public async Task CreateOrderAsync_masks_all_but_the_last_four_digits_of_the_card_number()
    {
        CreateOrderCommand? sentCommand = null;
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Do<IdentifiedCommand<CreateOrderCommand, bool>>(c => sentCommand = c.Command)).Returns(true);

        await OrdersApi.CreateOrderAsync(Guid.NewGuid(), CreateOrderRequest(cardNumber: "4111111111111111"), CreateServices(mediator));

        Assert.AreEqual("XXXXXXXXXXXX1111", sentCommand!.CardNumber);
    }

    [TestMethod]
    public async Task CreateOrderAsync_masks_short_card_numbers_without_throwing()
    {
        CreateOrderCommand? sentCommand = null;
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Do<IdentifiedCommand<CreateOrderCommand, bool>>(c => sentCommand = c.Command)).Returns(true);

        // Empty and very-short card numbers must not throw ArgumentOutOfRangeException
        // while still being fully masked.
        var result = await OrdersApi.CreateOrderAsync(Guid.NewGuid(), CreateOrderRequest(cardNumber: "12"), CreateServices(mediator));

        Assert.IsInstanceOfType<Ok>(result.Result);
        Assert.AreEqual("XX", sentCommand!.CardNumber);
    }

    // Real, if surprising, behavior: the endpoint only logs on inner-command failure, it
    // never returns anything other than Ok for it - a missing/empty request ID is the
    // only branch that returns something other than success.
    [TestMethod]
    public async Task CreateOrderAsync_returns_ok_even_when_the_inner_command_fails()
    {
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<IdentifiedCommand<CreateOrderCommand, bool>>()).Returns(false);

        var result = await OrdersApi.CreateOrderAsync(Guid.NewGuid(), CreateOrderRequest(), CreateServices(mediator));

        Assert.IsInstanceOfType<Ok>(result.Result);
    }
}
