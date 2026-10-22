using eShop.Ordering.Domain.Exceptions;
using eShop.Ordering.Infrastructure.Idempotency;
using MediatR;
using NSubstitute;

namespace eShop.Ordering.Infrastructure.UnitTests
{
    [TestClass]
    public class RequestManagerTests
    {
        [TestMethod]
        public async Task ExistAsync_returns_false_when_no_request_with_that_id_exists()
        {
            var manager = new RequestManager(TestOrderingContext.Create(Substitute.For<IMediator>()));

            var exists = await manager.ExistAsync(Guid.NewGuid());

            Assert.IsFalse(exists);
        }

        [TestMethod]
        public async Task CreateRequestForCommandAsync_persists_a_new_request()
        {
            var manager = new RequestManager(TestOrderingContext.Create(Substitute.For<IMediator>()));
            var id = Guid.NewGuid();

            await manager.CreateRequestForCommandAsync<string>(id);

            Assert.IsTrue(await manager.ExistAsync(id));
        }

        [TestMethod]
        public async Task CreateRequestForCommandAsync_throws_when_the_request_already_exists()
        {
            var manager = new RequestManager(TestOrderingContext.Create(Substitute.For<IMediator>()));
            var id = Guid.NewGuid();
            await manager.CreateRequestForCommandAsync<string>(id);

            await Assert.ThrowsExactlyAsync<OrderingDomainException>(
                () => manager.CreateRequestForCommandAsync<string>(id));
        }
    }
}
