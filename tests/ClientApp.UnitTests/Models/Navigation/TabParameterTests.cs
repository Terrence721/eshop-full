using eShop.ClientApp.Models.Navigation;

namespace eShop.ClientApp.UnitTests.Models.Navigation
{
    [TestClass]
    public class TabParameterTests
    {
        [TestMethod]
        public void Default_TabIndex_IsZero()
        {
            var parameter = new TabParameter();

            Assert.AreEqual(0, parameter.TabIndex);
        }

        [TestMethod]
        public void TabIndex_CanBeSet()
        {
            var parameter = new TabParameter { TabIndex = 3 };

            Assert.AreEqual(3, parameter.TabIndex);
        }
    }
}
