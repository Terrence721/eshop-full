using System.ComponentModel.DataAnnotations;
using eShop.Basket.API.Model;

namespace eShop.Basket.API.UnitTests;

[TestClass]
public class BasketItemTests
{
    [TestMethod]
    public void Validate_returns_an_error_when_quantity_is_zero()
    {
        var item = new BasketItem { Quantity = 0 };

        var results = item.Validate(new ValidationContext(item)).ToList();

        Assert.HasCount(1, results);
        Assert.AreEqual("Invalid number of units", results[0].ErrorMessage);
        CollectionAssert.Contains(results[0].MemberNames.ToList(), "Quantity");
    }

    [TestMethod]
    public void Validate_returns_an_error_when_quantity_is_negative()
    {
        var item = new BasketItem { Quantity = -1 };

        var results = item.Validate(new ValidationContext(item)).ToList();

        Assert.HasCount(1, results);
    }

    [TestMethod]
    public void Validate_returns_no_errors_when_quantity_is_positive()
    {
        var item = new BasketItem { Quantity = 1 };

        var results = item.Validate(new ValidationContext(item)).ToList();

        Assert.IsEmpty(results);
    }
}
