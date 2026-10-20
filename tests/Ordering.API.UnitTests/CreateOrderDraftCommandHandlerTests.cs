using eShop.Ordering.API.Application.Commands;
using eShop.Ordering.API.Application.Models;

namespace eShop.Ordering.API.UnitTests;

[TestClass]
public class CreateOrderDraftCommandHandlerTests
{
    private static BasketItem[] CreateItems() =>
    [
        new() { ProductId = 1, ProductName = "Widget", PictureUrl = "widget.png", UnitPrice = 10.00m, Quantity = 2 },
        new() { ProductId = 2, ProductName = "Gadget", PictureUrl = "gadget.png", UnitPrice = 5.00m, Quantity = 1 },
    ];

    [TestMethod]
    public async Task Handle_builds_a_draft_with_every_item()
    {
        var handler = new CreateOrderDraftCommandHandler();

        var draft = await handler.Handle(new CreateOrderDraftCommand("buyer-1", CreateItems()), CancellationToken.None);

        Assert.HasCount(2, draft.OrderItems.ToList());
    }

    [TestMethod]
    public async Task Handle_computes_the_correct_total()
    {
        var handler = new CreateOrderDraftCommandHandler();

        var draft = await handler.Handle(new CreateOrderDraftCommand("buyer-1", CreateItems()), CancellationToken.None);

        Assert.AreEqual(25.00m, draft.Total);
    }
}
