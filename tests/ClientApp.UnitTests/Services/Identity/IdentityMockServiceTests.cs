using eShop.ClientApp.Models.User;
using eShop.ClientApp.Services.Identity;

namespace eShop.ClientApp.UnitTests.Services.Identity
{
    [TestClass]
    public class IdentityMockServiceTests
    {
        [TestMethod]
        public async Task SignIn_SetsSignedIn()
        {
            var service = new IdentityMockService();

            var result = await service.SignInAsync();

            Assert.IsTrue(result);
            Assert.AreEqual("sampleUser", (await service.GetUserInfoAsync()).PreferredUsername);
        }

        [TestMethod]
        public async Task SignOut_ClearsSignedIn()
        {
            var service = new IdentityMockService();
            await service.SignInAsync();

            var result = await service.SignOutAsync();

            Assert.IsFalse(result);
            Assert.AreEqual(UserInfo.Default, await service.GetUserInfoAsync());
        }

        [TestMethod]
        public async Task GetAuthToken_WhenSignedOut_IsEmpty()
        {
            var service = new IdentityMockService();

            var token = await service.GetAuthTokenAsync();

            Assert.AreEqual(string.Empty, token);
        }
    }
}
