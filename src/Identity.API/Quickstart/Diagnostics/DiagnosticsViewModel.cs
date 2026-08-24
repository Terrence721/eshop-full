// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

namespace IdentityServerHost.Quickstart.UI;

public class DiagnosticsViewModel
{
    public required IEnumerable<ClaimViewModel> Claims { get; set; }
    public required IDictionary<string, string?> Properties { get; set; }
    public required IEnumerable<string> Clients { get; set; }
}

public class ClaimViewModel
{
    public required string Type { get; set; }
    public required string Value { get; set; }
}
