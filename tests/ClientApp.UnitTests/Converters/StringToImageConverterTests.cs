using Avalonia.Media.Imaging;
using eShop.ClientApp.Converters;

namespace eShop.ClientApp.UnitTests.Converters
{
    [TestClass]
    public class StringToImageConverterTests
    {
        [TestMethod]
        public void Convert_NullOrEmpty_ReturnsNull()
        {
            var converter = StringToImageConverter.Instance;

            Assert.IsNull(converter.Convert(null, typeof(Bitmap), null, System.Globalization.CultureInfo.InvariantCulture));
            Assert.IsNull(converter.Convert(string.Empty, typeof(Bitmap), null, System.Globalization.CultureInfo.InvariantCulture));
            Assert.IsNull(converter.Convert("   ", typeof(Bitmap), null, System.Globalization.CultureInfo.InvariantCulture));
        }

        [TestMethod]
        public void Convert_UnknownAssetUri_ReturnsNull()
        {
            var converter = StringToImageConverter.Instance;

            var result = converter.Convert("avares://ClientApp/Assets/does-not-exist.png", typeof(Bitmap), null, System.Globalization.CultureInfo.InvariantCulture);

            Assert.IsNull(result);
        }

        [TestMethod]
        public void Convert_InvalidHttpUrl_ReturnsNull()
        {
            var converter = StringToImageConverter.Instance;

            var result = converter.Convert("http://127.0.0.1:1/does-not-exist.png", typeof(Bitmap), null, System.Globalization.CultureInfo.InvariantCulture);

            Assert.IsNull(result);
        }

        [TestMethod]
        public void Convert_SameUnknownUri_ReturnsCachedInstance()
        {
            var converter = StringToImageConverter.Instance;

            var first = converter.Convert("avares://ClientApp/Assets/unknown-cache-test.png", typeof(Bitmap), null, System.Globalization.CultureInfo.InvariantCulture);
            var second = converter.Convert("avares://ClientApp/Assets/unknown-cache-test.png", typeof(Bitmap), null, System.Globalization.CultureInfo.InvariantCulture);

            Assert.AreSame(first, second);
        }
    }
}
