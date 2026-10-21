// Require a public Program class to implement the
// fixture for the WebApplicationFactory in the
// integration tests. Using IVT is not sufficient
// in this case, because the accessibility of the
// `Program` type is checked.
/// <summary>
/// Partial <see cref="Program" /> exposed publicly for WebApplicationFactory-based integration tests.
/// </summary>
public partial class Program { }
