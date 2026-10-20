using eShop.Catalog.API.Model;
using eShop.Catalog.API.Services;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NSubstitute;

namespace eShop.Catalog.API.UnitTests;

[TestClass]
public class CatalogAITests
{
    private static CatalogAI CreateCatalogAI(int embeddingDimensions, IEmbeddingGenerator<string, Embedding<float>>? generator)
        => new(Options.Create(new CatalogOptions { EmbeddingDimensions = embeddingDimensions }), Substitute.For<ILogger<CatalogAI>>(), generator);

    [TestMethod]
    public void IsEnabled_false_when_no_embedding_generator_is_configured()
    {
        var catalogAI = CreateCatalogAI(384, null);

        Assert.IsFalse(catalogAI.IsEnabled);
    }

    [TestMethod]
    public void IsEnabled_true_when_an_embedding_generator_is_configured()
    {
        var catalogAI = CreateCatalogAI(384, Substitute.For<IEmbeddingGenerator<string, Embedding<float>>>());

        Assert.IsTrue(catalogAI.IsEnabled);
    }

    [TestMethod]
    public async Task GetEmbeddingAsync_item_returns_null_when_disabled()
    {
        var catalogAI = CreateCatalogAI(384, null);

        var result = await catalogAI.GetEmbeddingAsync(new CatalogItem("Widget"));

        Assert.IsNull(result);
    }

    [TestMethod]
    public async Task GetEmbeddingAsync_text_returns_null_when_disabled()
    {
        var catalogAI = CreateCatalogAI(384, null);

        var result = await catalogAI.GetEmbeddingAsync("some text");

        Assert.IsNull(result);
    }

    [TestMethod]
    public async Task GetEmbeddingsAsync_returns_null_when_disabled()
    {
        var catalogAI = CreateCatalogAI(384, null);

        var result = await catalogAI.GetEmbeddingsAsync([new CatalogItem("Widget")]);

        Assert.IsNull(result);
    }

    [TestMethod]
    public async Task GetEmbeddingAsync_text_truncates_the_generated_vector_to_the_configured_dimensions()
    {
        var generator = Substitute.For<IEmbeddingGenerator<string, Embedding<float>>>();
        generator.GenerateAsync(Arg.Any<IEnumerable<string>>(), Arg.Any<EmbeddingGenerationOptions>(), Arg.Any<CancellationToken>())
            .Returns(callInfo =>
            {
                var count = callInfo.Arg<IEnumerable<string>>().Count();
                var embeddings = Enumerable.Range(0, count).Select(_ => new Embedding<float>(new float[] { 1f, 2f, 3f, 4f, 5f }));
                return Task.FromResult(new GeneratedEmbeddings<Embedding<float>>(embeddings));
            });
        var catalogAI = CreateCatalogAI(3, generator);

        var result = await catalogAI.GetEmbeddingAsync("some text");

        CollectionAssert.AreEqual(new float[] { 1f, 2f, 3f }, result!.ToArray());
    }

    [TestMethod]
    public async Task GetEmbeddingAsync_item_combines_name_and_description_before_generating()
    {
        var generator = Substitute.For<IEmbeddingGenerator<string, Embedding<float>>>();
        string? capturedInput = null;
        generator.GenerateAsync(Arg.Any<IEnumerable<string>>(), Arg.Any<EmbeddingGenerationOptions>(), Arg.Any<CancellationToken>())
            .Returns(callInfo =>
            {
                capturedInput = callInfo.Arg<IEnumerable<string>>().Single();
                return Task.FromResult(new GeneratedEmbeddings<Embedding<float>>([new Embedding<float>(new float[] { 1f })]));
            });
        var catalogAI = CreateCatalogAI(1, generator);
        var item = new CatalogItem("Widget") { Description = "A useful widget" };

        await catalogAI.GetEmbeddingAsync(item);

        Assert.AreEqual("Widget A useful widget", capturedInput);
    }

    [TestMethod]
    public async Task GetEmbeddingsAsync_truncates_every_items_vector_to_the_configured_dimensions()
    {
        var generator = Substitute.For<IEmbeddingGenerator<string, Embedding<float>>>();
        generator.GenerateAsync(Arg.Any<IEnumerable<string>>(), Arg.Any<EmbeddingGenerationOptions>(), Arg.Any<CancellationToken>())
            .Returns(callInfo =>
            {
                var count = callInfo.Arg<IEnumerable<string>>().Count();
                var embeddings = Enumerable.Range(0, count).Select(_ => new Embedding<float>(new float[] { 1f, 2f, 3f, 4f }));
                return Task.FromResult(new GeneratedEmbeddings<Embedding<float>>(embeddings));
            });
        var catalogAI = CreateCatalogAI(2, generator);
        var items = new[] { new CatalogItem("A"), new CatalogItem("B") };

        var result = await catalogAI.GetEmbeddingsAsync(items);

        Assert.AreEqual(2, result!.Count);
        CollectionAssert.AreEqual(new float[] { 1f, 2f }, result[0].ToArray());
        CollectionAssert.AreEqual(new float[] { 1f, 2f }, result[1].ToArray());
    }
}
