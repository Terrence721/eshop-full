using eShop.Ordering.API.Application.Models;
using eShop.Ordering.API.Extensions;

namespace eShop.Ordering.API.UnitTests;

[TestClass]
public class BasketItemExtensionsTests
{
    [TestMethod]
    public void ToOrderItemDTO_maps_every_field()
    {
        var item = new BasketItem
        {
            ProductId = 1,
            ProductName = "Widget",
            PictureUrl = "widget.png",
            UnitPrice = 10.00m,
            Quantity = 3,
        };

        var dto = item.ToOrderItemDTO();

        Assert.AreEqual(1, dto.ProductId);
        Assert.AreEqual("Widget", dto.ProductName);
        Assert.AreEqual("widget.png", dto.PictureUrl);
        Assert.AreEqual(10.00m, dto.UnitPrice);
        Assert.AreEqual(3, dto.Units);
    }

    [TestMethod]
    public void ToOrderItemsDTO_maps_every_item_in_the_sequence()
    {
        var items = new[]
        {
            new BasketItem { ProductId = 1, ProductName = "Widget", PictureUrl = "widget.png", UnitPrice = 10.00m, Quantity = 1 },
            new BasketItem { ProductId = 2, ProductName = "Gadget", PictureUrl = "gadget.png", UnitPrice = 5.00m, Quantity = 2 },
        };

        var dtos = items.ToOrderItemsDTO().ToList();

        Assert.HasCount(2, dtos);
        Assert.AreEqual("Widget", dtos[0].ProductName);
        Assert.AreEqual("Gadget", dtos[1].ProductName);
    }
}
