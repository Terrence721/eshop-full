using eShop.Ordering.API.Extensions;

namespace eShop.Ordering.API.UnitTests;

[TestClass]
public class LinqSelectExtensionsTests
{
    [TestMethod]
    public void SelectTry_returns_a_successful_result_for_every_element_when_the_selector_never_throws()
    {
        var results = new[] { 1, 2, 3 }.SelectTry(x => x * 2).ToList();

        Assert.HasCount(3, results);
        Assert.IsTrue(results.All(r => r.CaughtException == null));
        CollectionAssert.AreEqual(new[] { 2, 4, 6 }, results.Select(r => r.Result).ToList());
    }

    [TestMethod]
    public void SelectTry_captures_the_exception_when_the_selector_throws_for_an_element()
    {
        var results = new[] { 1, 0, 3 }.SelectTry(x => 10 / x).ToList();

        Assert.IsNotNull(results[1].CaughtException);
        Assert.IsInstanceOfType<DivideByZeroException>(results[1].CaughtException);
    }

    [TestMethod]
    public void SelectTry_continues_processing_remaining_elements_after_a_caught_exception()
    {
        var results = new[] { 1, 0, 3 }.SelectTry(x => 10 / x).ToList();

        Assert.HasCount(3, results);
        Assert.AreEqual(10, results[0].Result);
        Assert.IsNull(results[0].CaughtException);
        Assert.AreEqual(3, results[2].Source);
        Assert.IsNull(results[2].CaughtException);
    }

    [TestMethod]
    public void OnCaughtException_single_arg_returns_the_result_when_no_exception_was_caught()
    {
        var results = new[] { 1, 2 }.SelectTry(x => x * 2).OnCaughtException(ex => -1).ToList();

        CollectionAssert.AreEqual(new[] { 2, 4 }, results);
    }

    [TestMethod]
    public void OnCaughtException_single_arg_invokes_the_handler_with_the_caught_exception()
    {
        var results = new[] { 1, 0 }.SelectTry(x => 10 / x).OnCaughtException(ex => -1).ToList();

        CollectionAssert.AreEqual(new[] { 10, -1 }, results);
    }

    [TestMethod]
    public void OnCaughtException_two_arg_invokes_the_handler_with_the_source_and_exception()
    {
        var results = new[] { 1, 0 }.SelectTry(x => 10 / x)
            .OnCaughtException((source, ex) => source)
            .ToList();

        CollectionAssert.AreEqual(new[] { 10, 0 }, results);
    }
}
