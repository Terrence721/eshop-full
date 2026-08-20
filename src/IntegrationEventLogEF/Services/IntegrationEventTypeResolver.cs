namespace eShop.IntegrationEventLogEF.Services;

internal static class IntegrationEventTypeResolver
{
    private static readonly Lazy<IReadOnlyDictionary<string, Type>> s_eventTypesByShortName =
        new(() => BuildEventTypesByShortName(GetIntegrationEventTypesFromEntryAssembly()));

    public static Type Resolve(string eventTypeShortName) => Resolve(s_eventTypesByShortName.Value, eventTypeShortName);

    internal static Type Resolve(IReadOnlyDictionary<string, Type> eventTypesByShortName, string eventTypeShortName)
    {
        if (!eventTypesByShortName.TryGetValue(eventTypeShortName, out var eventType))
        {
            throw new InvalidOperationException(
                $"No integration event type named '{eventTypeShortName}' was found in the entry assembly.");
        }

        return eventType;
    }

    internal static IReadOnlyDictionary<string, Type> BuildEventTypesByShortName(IEnumerable<Type> candidateTypes)
    {
        var eventTypesByShortName = new Dictionary<string, Type>();
        var duplicateNames = new List<string>();

        foreach (var type in candidateTypes)
        {
            if (!eventTypesByShortName.TryAdd(type.Name, type))
            {
                duplicateNames.Add(type.Name);
            }
        }

        if (duplicateNames.Count != 0)
        {
            throw new InvalidOperationException(
                $"Multiple integration event types share the same short name, which the outbox can't disambiguate: {string.Join(", ", duplicateNames)}.");
        }

        return eventTypesByShortName;
    }

    private static IEnumerable<Type> GetIntegrationEventTypesFromEntryAssembly()
    {
        var entryAssembly = Assembly.GetEntryAssembly()
            ?? throw new InvalidOperationException("No entry assembly is available to resolve integration event types from.");

        return entryAssembly.GetTypes().Where(t => t.Name.EndsWith(nameof(IntegrationEvent), StringComparison.Ordinal));
    }
}
