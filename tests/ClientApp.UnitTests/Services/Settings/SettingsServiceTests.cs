using eShop.ClientApp.Services.Settings;
using System.Reflection;

namespace eShop.ClientApp.UnitTests.Services.Settings
{
    [TestClass]
    public class SettingsServiceTests
    {
        private static void ResetStore()
        {
            var type = typeof(SettingsService).Assembly.GetType("eShop.ClientApp.Services.Settings.LocalSettingsStore")!;
            var loadedField = type.GetField("_loaded", BindingFlags.Static | BindingFlags.NonPublic)!;
            loadedField.SetValue(null, false);

            var cacheField = type.GetField("Cache", BindingFlags.Static | BindingFlags.NonPublic)!;
            ((Dictionary<string, string>)cacheField.GetValue(null)!).Clear();

            var pathField = type.GetField("FilePath", BindingFlags.Static | BindingFlags.NonPublic)!;
            var path = (string?)pathField.GetValue(null);
            if (!string.IsNullOrEmpty(path) && File.Exists(path))
            {
                File.Delete(path);
            }
        }

        [TestInitialize]
        public void Init() => ResetStore();

        [TestMethod]
        public void UseMocks_DefaultsToTrue()
        {
            var settings = new SettingsService();

            Assert.IsTrue(settings.UseMocks);
        }

        [TestMethod]
        public void UseMocks_CanBeToggled()
        {
            var settings = new SettingsService();

            settings.UseMocks = false;

            Assert.IsFalse(settings.UseMocks);
        }

        [TestMethod]
        public void IdentityEndpointBase_DefaultsToEmpty()
        {
            var settings = new SettingsService();

            Assert.AreEqual(string.Empty, settings.IdentityEndpointBase);
        }

        [TestMethod]
        public void IdentityEndpointBase_RoundTrips()
        {
            var settings = new SettingsService();

            settings.IdentityEndpointBase = "http://identity:8080";

            Assert.AreEqual("http://identity:8080", settings.IdentityEndpointBase);
        }

        [TestMethod]
        public void UseFakeLocation_DefaultsToFalse()
        {
            var settings = new SettingsService();

            Assert.IsFalse(settings.UseFakeLocation);
        }

        [TestMethod]
        public void Latitude_DefaultsToFakeLatitude()
        {
            var settings = new SettingsService();

            Assert.AreEqual("47.60461", settings.Latitude);
        }
    }
}
