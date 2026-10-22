using eShop.Ordering.Domain.AggregatesModel.OrderAggregate;
using eShop.Ordering.Infrastructure.Repositories;
using MediatR;
using Microsoft.EntityFrameworkCore;
using NSubstitute;

namespace eShop.Ordering.Infrastructure.UnitTests
{
    [TestClass]
    public class OrderRepositoryTests
    {
        private static readonly DateTime FutureExpiration = DateTime.UtcNow.AddYears(1);

        private static Order CreateOrder() =>
            new("user-1", "Alice", new Address("123 Main St", "Springfield", "IL", "USA", "62704"),
                1, "4111111111111111", "123", "Alice", FutureExpiration);

        [TestMethod]
        public void UnitOfWork_returns_the_underlying_context()
        {
            var context = TestOrderingContext.Create(Substitute.For<IMediator>());
            var repository = new OrderRepository(context);

            Assert.AreSame(context, repository.UnitOfWork);
        }

        [TestMethod]
        public void Add_adds_the_order_to_the_context()
        {
            var context = TestOrderingContext.Create(Substitute.For<IMediator>());
            var repository = new OrderRepository(context);
            var order = CreateOrder();

            repository.Add(order);

            Assert.AreEqual(EntityState.Added, context.Entry(order).State);
        }

        [TestMethod]
        public async Task GetAsync_returns_the_order_with_its_items_loaded()
        {
            var dbName = Guid.NewGuid().ToString();
            var mediator = Substitute.For<IMediator>();
            var writeContext = TestOrderingContext.CreateNamed(dbName, mediator);
            var order = CreateOrder();
            order.AddOrderItem(1, "Widget", 10.00m, 0, "widget.png", 2);
            new OrderRepository(writeContext).Add(order);
            await writeContext.SaveChangesAsync();

            var readContext = TestOrderingContext.CreateNamed(dbName, mediator);
            var found = await new OrderRepository(readContext).GetAsync(order.Id);

            Assert.IsNotNull(found);
            Assert.HasCount(1, found.OrderItems);
            Assert.AreEqual("Widget", found.OrderItems.Single().ProductName);
        }

        [TestMethod]
        public async Task GetAsync_returns_null_when_no_order_matches()
        {
            var context = TestOrderingContext.Create(Substitute.For<IMediator>());
            var repository = new OrderRepository(context);

            var found = await repository.GetAsync(999);

            Assert.IsNull(found);
        }

        [TestMethod]
        public void Update_marks_the_order_as_modified()
        {
            var context = TestOrderingContext.Create(Substitute.For<IMediator>());
            var repository = new OrderRepository(context);
            var order = CreateOrder();

            repository.Update(order);

            Assert.AreEqual(EntityState.Modified, context.Entry(order).State);
        }
    }
}
