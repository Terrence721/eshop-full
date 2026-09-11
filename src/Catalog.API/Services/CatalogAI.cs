using System.Diagnostics;
using Microsoft.Extensions.AI;
using Pgvector;

namespace eShop.Catalog.API.Services;

public sealed class CatalogAI : ICatalogAI
{
    private readonly int _embeddingDimensions;
    private readonly IEmbeddingGenerator<string, Embedding<float>>? _embeddingGenerator;

    /// <summary>Logger for use in AI operations.</summary>
    private readonly ILogger _logger;

    public CatalogAI(IOptions<CatalogOptions> options, ILogger<CatalogAI> logger, IEmbeddingGenerator<string, Embedding<float>>? embeddingGenerator = null)
    {
        _embeddingDimensions = options.Value.EmbeddingDimensions;
        _embeddingGenerator = embeddingGenerator;
        _logger = logger;
    }

    /// <inheritdoc/>
    public bool IsEnabled => _embeddingGenerator is not null;

    /// <inheritdoc/>
    public ValueTask<Vector?> GetEmbeddingAsync(CatalogItem item) =>
        IsEnabled ?
            GetEmbeddingAsync(CatalogItemToString(item)) :
            ValueTask.FromResult<Vector?>(null);

    /// <inheritdoc/>
    public async ValueTask<IReadOnlyList<Vector>?> GetEmbeddingsAsync(IEnumerable<CatalogItem> items)
    {
        if (IsEnabled)
        {
            long timestamp = Stopwatch.GetTimestamp();

            GeneratedEmbeddings<Embedding<float>> embeddings = await _embeddingGenerator!.GenerateAsync(items.Select(CatalogItemToString));
            var results = embeddings.Select(m => new Vector(m.Vector[0.._embeddingDimensions])).ToList();

            if (_logger.IsEnabled(LogLevel.Trace))
            {
                _logger.LogTrace("Generated {EmbeddingsCount} embeddings in {ElapsedMilliseconds}s", results.Count, Stopwatch.GetElapsedTime(timestamp).TotalSeconds);
            }

            return results;
        }

        return null;
    }

    /// <inheritdoc/>
    public async ValueTask<Vector?> GetEmbeddingAsync(string text)
    {
        if (IsEnabled)
        {
            long timestamp = Stopwatch.GetTimestamp();

            var embedding = await _embeddingGenerator!.GenerateVectorAsync(text);
            embedding = embedding[0.._embeddingDimensions];

            if (_logger.IsEnabled(LogLevel.Trace))
            {
                _logger.LogTrace("Generated embedding in {ElapsedMilliseconds}s: '{Text}'", Stopwatch.GetElapsedTime(timestamp).TotalSeconds, text);
            }

            return new Vector(embedding);
        }

        return null;
    }

    private static string CatalogItemToString(CatalogItem item) => $"{item.Name} {item.Description}";
}
