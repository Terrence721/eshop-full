using eShop.Ordering.API.Application.Behaviors;
using eShop.Ordering.API.Application.Commands;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace eShop.Ordering.API.UnitTests
{
    [TestClass]
    public class LoggingBehaviorTests
    {
        [TestMethod]
        public async Task Handle_calls_next_and_returns_its_response()
        {
            var behavior = new LoggingBehavior<CancelOrderCommand, bool>(
                Substitute.For<ILogger<LoggingBehavior<CancelOrderCommand, bool>>>());

            var result = await behavior.Handle(new CancelOrderCommand(1), ct => Task.FromResult(true), CancellationToken.None);

            Assert.IsTrue(result);
        }
    }
}
