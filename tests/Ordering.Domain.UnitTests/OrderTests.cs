using eShop.Ordering.Domain.AggregatesModel.OrderAggregate;
using eShop.Ordering.Domain.Events;
using eShop.Ordering.Domain.Exceptions;

namespace eShop.Ordering.Domain.UnitTests;

[TestClass]
public class OrderTests
{
    private static readonly DateTime FutureExpiration = DateTime.UtcNow.AddYears(1);

    private static Address CreateAddress() => new("123 Main St", "Springfield", "IL", "USA", "62704");

    private static Order CreateOrder() =>
        new("user-1", "Alice", CreateAddress(), 1, "4111111111111111", "123", "Alice", FutureExpiration);

    [TestMethod]
    public void NewDraft_creates_an_order_with_no_domain_events()
    {
        var order = Order.NewDraft();

        Assert.IsNull(order.DomainEvents);
    }

    [TestMethod]
    public void NewDraft_creates_an_order_with_no_items()
    {
        var order = Order.NewDraft();

        Assert.IsEmpty(order.OrderItems);
    }

    [TestMethod]
    public void Constructor_sets_the_initial_status_to_Submitted()
    {
        var order = CreateOrder();

        Assert.AreEqual(OrderStatus.Submitted, order.OrderStatus);
    }

    [TestMethod]
    public void Constructor_sets_the_address()
    {
        var address = CreateAddress();
        var order = new Order("user-1", "Alice", address, 1, "4111111111111111", "123", "Alice", FutureExpiration);

        Assert.AreSame(address, order.Address);
    }

    [TestMethod]
    public void Constructor_sets_the_order_date_to_now()
    {
        var before = DateTime.UtcNow;
        var order = CreateOrder();
        var after = DateTime.UtcNow;

        Assert.IsTrue(order.OrderDate >= before && order.OrderDate <= after);
    }

    [TestMethod]
    public void Constructor_raises_an_OrderStartedDomainEvent()
    {
        var order = CreateOrder();

        var raised = order.DomainEvents!.OfType<OrderStartedDomainEvent>().Single();
        Assert.AreSame(order, raised.Order);
        Assert.AreEqual("user-1", raised.UserId);
        Assert.AreEqual("Alice", raised.UserName);
    }

    [TestMethod]
    public void AddOrderItem_adds_a_new_item_when_none_matches()
    {
        var order = CreateOrder();

        order.AddOrderItem(1, "Widget", 10.00m, 0, "widget.png", 2);

        Assert.HasCount(1, order.OrderItems);
    }

    [TestMethod]
    public void AddOrderItem_merges_units_into_an_existing_item_for_the_same_product()
    {
        var order = CreateOrder();
        order.AddOrderItem(1, "Widget", 10.00m, 0, "widget.png", 2);

        order.AddOrderItem(1, "Widget", 10.00m, 0, "widget.png", 3);

        Assert.HasCount(1, order.OrderItems);
        Assert.AreEqual(5, order.OrderItems.Single().Units);
    }

    [TestMethod]
    public void AddOrderItem_raises_the_discount_when_the_new_discount_is_higher()
    {
        var order = CreateOrder();
        order.AddOrderItem(1, "Widget", 10.00m, 1.00m, "widget.png", 2);

        order.AddOrderItem(1, "Widget", 10.00m, 3.00m, "widget.png", 1);

        Assert.AreEqual(3.00m, order.OrderItems.Single().Discount);
    }

    [TestMethod]
    public void AddOrderItem_keeps_the_existing_discount_when_the_new_discount_is_lower()
    {
        var order = CreateOrder();
        order.AddOrderItem(1, "Widget", 10.00m, 5.00m, "widget.png", 2);

        order.AddOrderItem(1, "Widget", 10.00m, 1.00m, "widget.png", 1);

        Assert.AreEqual(5.00m, order.OrderItems.Single().Discount);
    }

    [TestMethod]
    public void SetPaymentMethodVerified_sets_BuyerId_and_PaymentId()
    {
        var order = CreateOrder();

        order.SetPaymentMethodVerified(42, 7);

        Assert.AreEqual(42, order.BuyerId);
        Assert.AreEqual(7, order.PaymentId);
    }

    [TestMethod]
    public void SetAwaitingValidationStatus_transitions_from_Submitted()
    {
        var order = CreateOrder();

        order.SetAwaitingValidationStatus();

        Assert.AreEqual(OrderStatus.AwaitingValidation, order.OrderStatus);
    }

    [TestMethod]
    public void SetAwaitingValidationStatus_raises_a_domain_event()
    {
        var order = CreateOrder();

        order.SetAwaitingValidationStatus();

        Assert.IsTrue(order.DomainEvents!.OfType<OrderStatusChangedToAwaitingValidationDomainEvent>().Any());
    }

    [TestMethod]
    public void SetAwaitingValidationStatus_is_a_no_op_from_a_non_Submitted_status()
    {
        var order = CreateOrder();
        order.SetAwaitingValidationStatus();
        order.SetStockConfirmedStatus();

        order.SetAwaitingValidationStatus();

        Assert.AreEqual(OrderStatus.StockConfirmed, order.OrderStatus);
    }

    [TestMethod]
    public void SetStockConfirmedStatus_transitions_from_AwaitingValidation_and_sets_a_description()
    {
        var order = CreateOrder();
        order.SetAwaitingValidationStatus();

        order.SetStockConfirmedStatus();

        Assert.AreEqual(OrderStatus.StockConfirmed, order.OrderStatus);
        Assert.AreEqual("All the items were confirmed with available stock.", order.Description);
    }

    [TestMethod]
    public void SetStockConfirmedStatus_is_a_no_op_from_Submitted()
    {
        var order = CreateOrder();

        order.SetStockConfirmedStatus();

        Assert.AreEqual(OrderStatus.Submitted, order.OrderStatus);
    }

    [TestMethod]
    public void SetPaidStatus_transitions_from_StockConfirmed_and_sets_a_description()
    {
        var order = CreateOrder();
        order.SetAwaitingValidationStatus();
        order.SetStockConfirmedStatus();

        order.SetPaidStatus();

        Assert.AreEqual(OrderStatus.Paid, order.OrderStatus);
        Assert.IsFalse(string.IsNullOrEmpty(order.Description));
    }

    [TestMethod]
    public void SetShippedStatus_throws_when_not_Paid()
    {
        var order = CreateOrder();

        Assert.ThrowsExactly<OrderingDomainException>(() => order.SetShippedStatus());
    }

    [TestMethod]
    public void SetShippedStatus_transitions_from_Paid_and_raises_a_domain_event()
    {
        var order = CreateOrder();
        order.SetAwaitingValidationStatus();
        order.SetStockConfirmedStatus();
        order.SetPaidStatus();

        order.SetShippedStatus();

        Assert.AreEqual(OrderStatus.Shipped, order.OrderStatus);
        Assert.IsTrue(order.DomainEvents!.OfType<OrderShippedDomainEvent>().Any());
    }

    [TestMethod]
    public void SetCancelledStatus_throws_when_Paid()
    {
        var order = CreateOrder();
        order.SetAwaitingValidationStatus();
        order.SetStockConfirmedStatus();
        order.SetPaidStatus();

        Assert.ThrowsExactly<OrderingDomainException>(() => order.SetCancelledStatus());
    }

    [TestMethod]
    public void SetCancelledStatus_throws_when_Shipped()
    {
        var order = CreateOrder();
        order.SetAwaitingValidationStatus();
        order.SetStockConfirmedStatus();
        order.SetPaidStatus();
        order.SetShippedStatus();

        Assert.ThrowsExactly<OrderingDomainException>(() => order.SetCancelledStatus());
    }

    [TestMethod]
    public void SetCancelledStatus_succeeds_from_Submitted_and_raises_a_domain_event()
    {
        var order = CreateOrder();

        order.SetCancelledStatus();

        Assert.AreEqual(OrderStatus.Cancelled, order.OrderStatus);
        Assert.IsTrue(order.DomainEvents!.OfType<OrderCancelledDomainEvent>().Any());
    }

    [TestMethod]
    public void SetCancelledStatusWhenStockIsRejected_cancels_from_AwaitingValidation_and_describes_rejected_items()
    {
        var order = CreateOrder();
        order.AddOrderItem(1, "Widget", 10.00m, 0, "widget.png", 2);
        order.AddOrderItem(2, "Gadget", 5.00m, 0, "gadget.png", 1);
        order.SetAwaitingValidationStatus();

        order.SetCancelledStatusWhenStockIsRejected([1]);

        Assert.AreEqual(OrderStatus.Cancelled, order.OrderStatus);
        StringAssert.Contains(order.Description!, "Widget");
    }

    [TestMethod]
    public void SetCancelledStatusWhenStockIsRejected_is_a_no_op_outside_AwaitingValidation()
    {
        var order = CreateOrder();

        order.SetCancelledStatusWhenStockIsRejected([1]);

        Assert.AreEqual(OrderStatus.Submitted, order.OrderStatus);
    }

    [TestMethod]
    public void GetTotal_sums_units_times_unit_price_across_items()
    {
        var order = CreateOrder();
        order.AddOrderItem(1, "Widget", 10.00m, 0, "widget.png", 2);
        order.AddOrderItem(2, "Gadget", 5.00m, 0, "gadget.png", 3);

        Assert.AreEqual(35.00m, order.GetTotal());
    }

    [TestMethod]
    public void GetTotal_is_zero_for_an_order_with_no_items()
    {
        var order = CreateOrder();

        Assert.AreEqual(0m, order.GetTotal());
    }
}
