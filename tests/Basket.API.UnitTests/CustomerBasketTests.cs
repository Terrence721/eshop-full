using eShop.Basket.API.Model;

namespace eShop.Basket.API.UnitTests;

[TestClass]
public class CustomerBasketTests
{
    [TestMethod]
    public void Default_constructor_leaves_BuyerId_null_and_Items_empty()
    {
        var basket = new CustomerBasket();

        Assert.IsNull(basket.BuyerId);
        Assert.IsEmpty(basket.Items);
    }

    [TestMethod]
    public void Constructor_with_customerId_sets_BuyerId()
    {
        var basket = new CustomerBasket("alice");

        Assert.AreEqual("alice", basket.BuyerId);
    }

    [TestMethod]
    public void Constructor_with_customerId_leaves_Items_empty()
    {
        var basket = new CustomerBasket("alice");

        Assert.IsEmpty(basket.Items);
    }
}
