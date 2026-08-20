using eShop.IntegrationEventLogEF.Utilities;

namespace IntegrationEventLogEF.UnitTests.Utilities;

[TestClass]
public class ResilientTransactionTests
{
    [TestMethod]
    public void New_throws_when_context_is_null()
    {
        Assert.ThrowsExactly<ArgumentNullException>(() => ResilientTransaction.New(null!));
    }
}
