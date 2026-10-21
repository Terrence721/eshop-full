// Require a public Program class to implement the
// fixture for the WebApplicationFactory in the
// integration tests. Using IVT is not sufficient
// in this case, because the accessibility of the
// `Program` type is checked.
// This marker type enables WebApplicationFactory<Program> in Catalog.API.FunctionalTests.
public partial class Program { }
