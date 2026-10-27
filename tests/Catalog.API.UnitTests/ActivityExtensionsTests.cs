using System.Diagnostics;

namespace eShop.Catalog.API.UnitTests
{
    [TestClass]
    public class ActivityExtensionsTests
    {
        [TestMethod]
        public void SetExceptionTags_does_nothing_when_activity_is_null()
        {
            var exception = new InvalidOperationException("boom");

            ActivityExtensions.SetExceptionTags(null, exception);

            // Reaching this line proves the no-op path did not throw.
        }

        [TestMethod]
        public void SetExceptionTags_sets_the_exception_tags_and_error_status()
        {
            using var activity = new Activity("test-activity").Start();
            var exception = new ArgumentOutOfRangeException("value", "value is out of range");

            activity.SetExceptionTags(exception);

            Assert.AreEqual(exception.Message, activity.GetTagItem("exception.message"));
            Assert.AreEqual(exception.ToString(), activity.GetTagItem("exception.stacktrace"));
            Assert.AreEqual(typeof(ArgumentOutOfRangeException).FullName, activity.GetTagItem("exception.type"));
            Assert.AreEqual(ActivityStatusCode.Error, activity.Status);
        }

        [TestMethod]
        public void SetExceptionTags_uses_the_exception_message_even_when_empty()
        {
            using var activity = new Activity("test-activity").Start();
            var exception = new Exception(string.Empty);

            activity.SetExceptionTags(exception);

            Assert.AreEqual(string.Empty, activity.GetTagItem("exception.message"));
            Assert.AreEqual(typeof(Exception).FullName, activity.GetTagItem("exception.type"));
            Assert.AreEqual(ActivityStatusCode.Error, activity.Status);
        }

        [TestMethod]
        public void SetExceptionTags_records_the_runtime_type_for_derived_exceptions()
        {
            using var activity = new Activity("test-activity").Start();
            Exception exception = new ArgumentException("bad argument");

            activity.SetExceptionTags(exception);

            Assert.AreEqual(typeof(ArgumentException).FullName, activity.GetTagItem("exception.type"));
        }

        [TestMethod]
        public void SetExceptionTags_records_nested_exception_details()
        {
            using var activity = new Activity("test-activity").Start();
            var inner = new InvalidOperationException("inner failure");
            var outer = new ApplicationException("outer failure", inner);

            activity.SetExceptionTags(outer);

            Assert.AreEqual("outer failure", activity.GetTagItem("exception.message"));
            Assert.AreEqual(outer.ToString(), activity.GetTagItem("exception.stacktrace"));
            Assert.AreEqual(typeof(ApplicationException).FullName, activity.GetTagItem("exception.type"));
            Assert.AreEqual(ActivityStatusCode.Error, activity.Status);
        }



    }
}
