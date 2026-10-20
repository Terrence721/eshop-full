// Strips CR/LF from user-controlled input before it reaches a log sink, so an
// attacker can't forge fake log lines by embedding newlines in a logged value
// (CodeQL cs/log-forging).
internal static class LogSanitizer
{
    public static string? ForLog(string? value) => value?.Replace("\r", string.Empty).Replace("\n", string.Empty);
}
