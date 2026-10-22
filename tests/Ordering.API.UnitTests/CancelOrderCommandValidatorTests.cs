using eShop.Ordering.API.Application.Commands;
using eShop.Ordering.API.Application.Validations;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace eShop.Ordering.API.UnitTests
{
    [TestClass]
    public class CancelOrderCommandValidatorTests
    {
        private static CancelOrderCommandValidator CreateValidator() =>
            new(Substitute.For<ILogger<CancelOrderCommandValidator>>());

        [TestMethod]
        public void Validate_succeeds_when_OrderNumber_is_nonzero()
        {
            var result = CreateValidator().Validate(new CancelOrderCommand(1));

            Assert.IsTrue(result.IsValid);
        }

        [TestMethod]
        public void Validate_fails_when_OrderNumber_is_zero()
        {
            var result = CreateValidator().Validate(new CancelOrderCommand(0));

            Assert.IsFalse(result.IsValid);
        }
    }
}
