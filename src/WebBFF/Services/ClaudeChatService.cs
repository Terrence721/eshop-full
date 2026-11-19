using System.Collections.Concurrent;
using System.Net.Http.Json;
using System.Runtime.CompilerServices;
using System.Text.Json.Serialization;

namespace eShop.WebBFF.Services
{
    internal record CatalogItemDetail(int Id, string Name, string Description, decimal Price, string? PictureUrl);
    internal interface ICatalogClient { Task<IReadOnlyList<CatalogItemDetail>> SearchAsync(string query); }

    internal class HttpCatalogClient(HttpClient http) : ICatalogClient
    {
        public async Task<IReadOnlyList<CatalogItemDetail>> SearchAsync(string query)
        {
            var url = $"api/catalog/items/withsemanticrelevance?text={Uri.EscapeDataString(query)}&PageSize=5";
            try
            {
                var resp = await http.GetAsync(url);
                if (resp.IsSuccessStatusCode)
                {
                    var items = await resp.Content.ReadFromJsonAsync<PaginatedItems>();
                    if (items?.Data != null && items.Data.Length > 0)
                        return items.Data.Select(MapToDetail).ToList();
                }
            } catch { }

            var fallbackUrl = $"api/catalog/items/by/{Uri.EscapeDataString(query)}?PageSize=5";
            using var fr = await http.GetAsync(fallbackUrl);
            if (fr.IsSuccessStatusCode)
            {
                var items = await fr.Content.ReadFromJsonAsync<PaginatedItems>();
                return items?.Data?.Select(MapToDetail).ToList() ?? [];
            }
            return [];
        }
        private static CatalogItemDetail MapToDetail(CatalogItem i) =>
            new(i.Id, i.Name, i.Description ?? "", i.Price,
                i.PictureFileName != null ? $"/remoteapi/catalog/api/catalog/items/{i.Id}/pic" : null);
        private record PaginatedItems([property: JsonPropertyName("data")] CatalogItem[] Data, int PageSize, int PageIndex, long Count);
        private record CatalogItem([property: JsonPropertyName("id")] int Id, [property: JsonPropertyName("name")] string Name,
                                    [property: JsonPropertyName("description")] string? Description,
                                    [property: JsonPropertyName("price")] decimal Price,
                                    [property: JsonPropertyName("pictureFileName")] string? PictureFileName,
                                    [property: JsonPropertyName("catalogTypeId")] int CatalogTypeId,
                                    [property: JsonPropertyName("catalogBrandId")] int CatalogBrandId);
    }

    internal sealed class ChatSession(ILogger logger)
    {
        private readonly List<(string Role, string Text)> _history = [];
        private readonly SemaphoreSlim _lock = new(1, 1);

        public void AddUserMessage(string text) { _lock.Wait(); try { _history.Add(("user", text)); } finally { _lock.Release(); } }
        public void AddAssistantMessage(string text) { _lock.Wait(); try { _history.Add(("assistant", text)); } finally { _lock.Release(); } }
        public IReadOnlyList<(string Role, string Text)> GetHistory() { _lock.Wait(); try { return [.. _history]; } finally { _lock.Release(); } }
        public void Clear() { _lock.Wait(); try { _history.Clear(); } finally { _lock.Release(); } }

        public async Task<string> RespondToAsync(string apiKey, HttpClient http)
        {
            await _lock.WaitAsync();
            try
            {
                while (true)
                {
                    var msgs = _history.Select(e => new { role = e.Role, content = e.Text }).ToList();

                    var bodyObj = new
                    {
                        model = "claude-haiku-4-5-20251201",
                        max_tokens = 1024,
                        messages = msgs,
                        tools = new[]
                        {
                            new
                            {
                                name = "catalog_search",
                                description = "Search the product catalog by name or description.",
                                input_schema = new
                                {
                                    type = "object",
                                    properties = new { query = new { type = "string", description = "Search query" } },
                                    required = new[] { "query" }
                                }
                            }
                        }
                    };

                    var body = System.Text.Json.JsonSerializer.Serialize(bodyObj);

                    using var req = new HttpRequestMessage(HttpMethod.Post, "https://api.anthropic.com/v1/messages")
                    {
                        Headers = { { "x-api-key", apiKey }, { "anthropic-version", "2023-06-01" } },
                        Content = new StringContent(body, System.Text.Encoding.UTF8, "application/json")
                    };
                    var resp = await http.SendAsync(req);
                    var responseStr = await resp.Content.ReadAsStringAsync();
                    var jdoc = System.Text.Json.JsonSerializer.Deserialize<System.Text.Json.JsonElement>(responseStr);
                    var contentArr = jdoc.GetProperty("content");
                    var textBlocks = contentArr.EnumerateArray().Where(b => b.GetProperty("type").GetString() == "text");
                    var toolUses = contentArr.EnumerateArray().Where(b => b.GetProperty("type").GetString() == "tool_use");

                    foreach (var tb in textBlocks)
                    {
                        var txt = tb.GetProperty("text").GetString() ?? "";
                        AddAssistantMessage(txt);
                        return txt;
                    }
                    foreach (var tu in toolUses)
                    {
                        var name = tu.GetProperty("name").GetString();
                        var input = tu.GetProperty("input");
                        if (name == "catalog_search")
                        {
                            var q = input.GetProperty("query").GetString() ?? "";
                            // Execute tool via injected catalog client
                            var result = await ExecuteToolAsync(q, http);
                            _history.Add(("assistant", result));
                        }
                    }
                }
            }
            finally { _lock.Release(); }
        }

        private async Task<string> ExecuteToolAsync(string query, HttpClient http)
        {
            try
            {
                var results = await new HttpCatalogClient(http).SearchAsync(query);
                if (!results.Any()) return "No products found matching your search.";
                var sb = new System.Text.StringBuilder();
                foreach (var item in results)
                    sb.AppendLine($"- ID: {item.Id} | {item.Name} | ${item.Price:F2} | {item.Description}");
                return $"Found {results.Count} products:\n{sb}";
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Catalog search failed for query: {Query}", query);
                return $"Error searching catalog: {ex.Message}";
            }
        }
    }

    public record ToolResult(string ToolCallId, string Content);
    public interface IClaudeChatService
    {
        Task<string> SendUserMessageAsync(string buyerGuid, string message);
        void ClearConversation(string buyerGuid);
        bool HasOngoingStreaming { get; }
        IAsyncEnumerable<string> StreamAssistantResponseAsync(string buyerGuid, string message, CancellationToken ct);
    }

    public class ClaudeChatService(IHttpClientFactory httpClientFactory, AppSettings appSettings, ILogger<ClaudeChatService> logger) : IClaudeChatService
    {
        private readonly ConcurrentDictionary<string, ChatSession> _sessions = new();
        public bool HasOngoingStreaming => false; // Streaming not yet implemented for this mode

        public async Task<string> SendUserMessageAsync(string buyerGuid, string message)
        {
            var session = GetOrCreateSession(buyerGuid);
            session.AddUserMessage(message);
            var key = appSettings.AnthropicApiKey ?? throw new InvalidOperationException("ClaudeChat:AnthropicApiKey not configured");
            var http = httpClientFactory.CreateClient("claude");
            return await session.RespondToAsync(key, http);
        }

        public async IAsyncEnumerable<string> StreamAssistantResponseAsync(string buyerGuid, string message, [EnumeratorCancellation] CancellationToken ct)
        {
            // Streaming not implemented yet - fallback to non-streaming response
            var response = await SendUserMessageAsync(buyerGuid, message);
            yield return response;
        }

        public void ClearConversation(string buyerGuid)
        {
            if (_sessions.TryRemove(buyerGuid, out var s)) s.Clear();
        }

        private ChatSession GetOrCreateSession(string buyerGuid) =>
            _sessions.GetOrAdd(buyerGuid, _ => new ChatSession(logger));
    }

    public record AppSettings(string? AnthropicApiKey);
}