using MediatR;
using Microsoft.EntityFrameworkCore;

namespace eShop.Ordering.Infrastructure.UnitTests;

internal static class TestOrderingContext
{
    public static OrderingContext Create(IMediator mediator) => CreateNamed(Guid.NewGuid().ToString(), mediator);

    public static OrderingContext CreateNamed(string dbName, IMediator mediator)
    {
        var options = new DbContextOptionsBuilder<OrderingContext>()
            .UseInMemoryDatabase(dbName)
            .Options;

        // OrderingContext's DbSet<T> properties are `required`, which forces
        // an object-initializer at a normal `new OrderingContext(...)` call
        // site - but assigning anything there runs AFTER the base
        // DbContext constructor's own auto-wiring and clobbers it with
        // null. Constructing via reflection runs the same real constructor
        // while sidestepping the compiler's required-member check
        // entirely - same technique already used for CatalogContext.
        return (OrderingContext)Activator.CreateInstance(typeof(OrderingContext), options, mediator)!;
    }
}
