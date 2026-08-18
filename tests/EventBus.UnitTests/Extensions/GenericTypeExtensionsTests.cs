using eShop.EventBus.Extensions;

namespace EventBus.UnitTests.Extensions;

[TestClass]
public class GenericTypeExtensionsTests
{
    [TestMethod]
    public void GetGenericTypeName_returns_simple_name_for_non_generic_type()
    {
        Assert.AreEqual("String", typeof(string).GetGenericTypeName());
    }

    [TestMethod]
    public void GetGenericTypeName_returns_pretty_name_with_single_type_argument()
    {
        Assert.AreEqual("List<Int32>", typeof(List<int>).GetGenericTypeName());
    }

    [TestMethod]
    public void GetGenericTypeName_returns_pretty_name_with_multiple_type_arguments()
    {
        Assert.AreEqual("Dictionary<String,Int32>", typeof(Dictionary<string, int>).GetGenericTypeName());
    }

    [TestMethod]
    public void GetGenericTypeName_object_overload_returns_simple_name_for_non_generic_instance()
    {
        Assert.AreEqual("String", "hello".GetGenericTypeName());
    }

    [TestMethod]
    public void GetGenericTypeName_object_overload_returns_pretty_name_for_generic_instance()
    {
        Assert.AreEqual("List<Int32>", new List<int>().GetGenericTypeName());
    }

    [TestMethod]
    public void GetGenericTypeName_does_not_recurse_into_nested_generic_arguments()
    {
        // Documents a known, deliberately-unfixed limitation (see todo.md's EventBus section):
        // a nested generic argument's own backtick-arity name leaks through raw, since
        // GetGenericArguments().Select(t => t.Name) calls the plain CLR Name, not GetGenericTypeName(),
        // on each argument. Nothing in this repo calls this with a nested generic today, so this test
        // exists to make any future change to that behavior deliberate, not silent.
        Assert.AreEqual("Dictionary<String,List`1>", typeof(Dictionary<string, List<int>>).GetGenericTypeName());
    }
}
