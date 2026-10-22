namespace Webhooks.API.Services;

/// <summary>
/// Verifies that a grant URL shares an origin with a webhook hook URL and echoes the expected token.
/// </summary>
public interface IGrantUrlTesterService
{
    /// <summary>
    /// Tests whether <paramref name="url" /> can be granted for <paramref name="urlHook" />.
    /// </summary>
    Task<bool> TestGrantUrl(string urlHook, string url, string token);
}