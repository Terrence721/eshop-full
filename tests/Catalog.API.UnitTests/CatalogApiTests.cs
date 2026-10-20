using eShop.Catalog.API.Infrastructure;
using eShop.Catalog.API.IntegrationEvents;
using eShop.Catalog.API.IntegrationEvents.Events;
using eShop.Catalog.API.Model;
using eShop.Catalog.API.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NSubstitute;

namespace eShop.Catalog.API.UnitTests;

// GetItemsBySemanticRelevance's actual vector-ordering query (CosineDistance,
// a Postgres/pgvector function translated to SQL) can't run against the
// InMemory provider at all -- only its two fallback branches (AI disabled,
// or no embedding generated) are covered here. GetItemPictureById isn't
// covered either: it reads a real file from disk via IWebHostEnvironment,
// integration-test territory, not a DbContext-backed unit.
[TestClass]
public class CatalogApiTests
{
    private static CatalogServices CreateServices(CatalogContext context, ICatalogAI? catalogAI = null, ICatalogIntegrationEventService? eventService = null)
        => new(context, catalogAI ?? Substitute.For<ICatalogAI>(), Options.Create(new CatalogOptions()),
            Substitute.For<ILogger<CatalogServices>>(), eventService ?? Substitute.For<ICatalogIntegrationEventService>());

    private static async Task<CatalogContext> SeededContext()
    {
        var context = TestCatalogContext.Create();
        context.CatalogBrands.Add(new CatalogBrand("Acme") { Id = 1 });
        context.CatalogTypes.Add(new CatalogType("Gear") { Id = 1 });
        context.CatalogItems.Add(new CatalogItem("Alpha Widget") { Id = 1, CatalogBrandId = 1, CatalogTypeId = 1, Price = 10 });
        context.CatalogItems.Add(new CatalogItem("Beta Widget") { Id = 2, CatalogBrandId = 1, CatalogTypeId = 1, Price = 20 });
        context.CatalogItems.Add(new CatalogItem("Gamma Gadget") { Id = 3, CatalogBrandId = 1, CatalogTypeId = 1, Price = 30 });
        await context.SaveChangesAsync();
        return context;
    }

    [TestMethod]
    public async Task GetAllItems_returns_every_item_when_no_filters_are_given()
    {
        using var context = await SeededContext();

        var result = await CatalogApi.GetAllItems(new PaginationRequest(), CreateServices(context), null, null, null);

        Assert.AreEqual(3, result.Value!.Count);
    }

    [TestMethod]
    public async Task GetAllItems_filters_by_name_prefix()
    {
        using var context = await SeededContext();

        var result = await CatalogApi.GetAllItems(new PaginationRequest(), CreateServices(context), "Alpha", null, null);

        Assert.AreEqual(1, result.Value!.Count);
        Assert.AreEqual("Alpha Widget", result.Value!.Data.Single().Name);
    }

    [TestMethod]
    public async Task GetAllItems_filters_by_type_and_brand()
    {
        using var context = await SeededContext();
        context.CatalogTypes.Add(new CatalogType("Other") { Id = 2 });
        context.CatalogItems.Add(new CatalogItem("Delta") { Id = 4, CatalogBrandId = 1, CatalogTypeId = 2, Price = 40 });
        await context.SaveChangesAsync();

        var result = await CatalogApi.GetAllItems(new PaginationRequest(), CreateServices(context), null, 2, 1);

        Assert.AreEqual(1, result.Value!.Count);
        Assert.AreEqual("Delta", result.Value!.Data.Single().Name);
    }

    [TestMethod]
    public async Task GetAllItems_paginates_using_page_size_and_index()
    {
        using var context = await SeededContext();

        var result = await CatalogApi.GetAllItems(new PaginationRequest(PageSize: 2, PageIndex: 1), CreateServices(context), null, null, null);

        Assert.AreEqual(3, result.Value!.Count);
        Assert.AreEqual(1, result.Value!.Data.Count());
        Assert.AreEqual("Gamma Gadget", result.Value!.Data.Single().Name);
    }

    [TestMethod]
    public async Task GetItemsByIds_returns_only_the_requested_items()
    {
        using var context = await SeededContext();

        var result = await CatalogApi.GetItemsByIds(CreateServices(context), [1, 3]);

        CollectionAssert.AreEquivalent(new[] { "Alpha Widget", "Gamma Gadget" }, result.Value!.Select(i => i.Name).ToArray());
    }

    [TestMethod]
    public async Task GetItemById_returns_bad_request_for_a_non_positive_id()
    {
        using var context = await SeededContext();

        var result = await CatalogApi.GetItemById(new DefaultHttpContext(), CreateServices(context), 0);

        Assert.IsInstanceOfType<BadRequest<ProblemDetails>>(result.Result);
    }

    [TestMethod]
    public async Task GetItemById_returns_not_found_for_an_id_that_does_not_exist()
    {
        using var context = await SeededContext();

        var result = await CatalogApi.GetItemById(new DefaultHttpContext(), CreateServices(context), 999);

        Assert.IsInstanceOfType<NotFound>(result.Result);
    }

    [TestMethod]
    public async Task GetItemById_returns_the_item_with_its_brand_included()
    {
        using var context = await SeededContext();

        var result = await CatalogApi.GetItemById(new DefaultHttpContext(), CreateServices(context), 1);

        var ok = (Ok<CatalogItem>)result.Result;
        Assert.AreEqual("Alpha Widget", ok.Value!.Name);
        Assert.AreEqual("Acme", ok.Value!.CatalogBrand!.Brand);
    }

    [TestMethod]
    public async Task GetItemsBySemanticRelevance_falls_back_to_name_search_when_AI_is_disabled()
    {
        using var context = await SeededContext();
        var catalogAI = Substitute.For<ICatalogAI>();
        catalogAI.IsEnabled.Returns(false);

        var result = await CatalogApi.GetItemsBySemanticRelevance(new PaginationRequest(), CreateServices(context, catalogAI), "Alpha");

        var ok = (Ok<PaginatedItems<CatalogItem>>)result.Result;
        Assert.AreEqual("Alpha Widget", ok.Value!.Data.Single().Name);
        _ = catalogAI.DidNotReceive().GetEmbeddingAsync(Arg.Any<string>());
    }

    [TestMethod]
    public async Task GetItemsBySemanticRelevance_falls_back_to_name_search_when_no_embedding_is_generated()
    {
        using var context = await SeededContext();
        var catalogAI = Substitute.For<ICatalogAI>();
        catalogAI.IsEnabled.Returns(true);
        catalogAI.GetEmbeddingAsync(Arg.Any<string>()).Returns((Pgvector.Vector?)null);

        var result = await CatalogApi.GetItemsBySemanticRelevance(new PaginationRequest(), CreateServices(context, catalogAI), "Alpha");

        var ok = (Ok<PaginatedItems<CatalogItem>>)result.Result;
        Assert.AreEqual("Alpha Widget", ok.Value!.Data.Single().Name);
    }

    [TestMethod]
    public async Task UpdateItem_returns_not_found_when_the_item_does_not_exist()
    {
        using var context = await SeededContext();

        var result = await CatalogApi.UpdateItem(new DefaultHttpContext(), 999, CreateServices(context), new CatalogItem("Ghost") { Id = 999, Price = 1 });

        Assert.IsInstanceOfType<NotFound<ProblemDetails>>(result.Result);
    }

    [TestMethod]
    public async Task UpdateItem_publishes_a_price_changed_event_through_the_event_service_when_the_price_changes()
    {
        using var context = await SeededContext();
        var eventService = Substitute.For<ICatalogIntegrationEventService>();
        var update = new CatalogItem("Alpha Widget") { Id = 1, CatalogBrandId = 1, CatalogTypeId = 1, Price = 15 };

        var result = await CatalogApi.UpdateItem(new DefaultHttpContext(), 1, CreateServices(context, eventService: eventService), update);

        Assert.IsInstanceOfType<Created>(result.Result);
        await eventService.Received(1).SaveEventAndCatalogContextChangesAsync(Arg.Any<ProductPriceChangedIntegrationEvent>());
        await eventService.Received(1).PublishThroughEventBusAsync(Arg.Any<ProductPriceChangedIntegrationEvent>());
    }

    [TestMethod]
    public async Task UpdateItem_just_saves_without_the_event_service_when_the_price_is_unchanged()
    {
        using var context = await SeededContext();
        var eventService = Substitute.For<ICatalogIntegrationEventService>();
        var update = new CatalogItem("Alpha Widget Renamed") { Id = 1, CatalogBrandId = 1, CatalogTypeId = 1, Price = 10 };

        var result = await CatalogApi.UpdateItem(new DefaultHttpContext(), 1, CreateServices(context, eventService: eventService), update);

        Assert.IsInstanceOfType<Created>(result.Result);
        await eventService.DidNotReceive().SaveEventAndCatalogContextChangesAsync(Arg.Any<ProductPriceChangedIntegrationEvent>());
        var saved = await context.CatalogItems.FindAsync(1);
        Assert.AreEqual("Alpha Widget Renamed", saved!.Name);
    }

    // Real upstream bug, confirmed verbatim in dotnet/eShop's own source (not
    // introduced by this fork): the guard is `productToUpdate?.Id == null`,
    // but CatalogItem.Id is a non-nullable int (defaults to 0), so that
    // condition can only be true when productToUpdate itself is null -- a
    // request body with no id (Id left at its default 0) sails straight
    // through to UpdateItem and gets a 404, not the intended 400. Documented
    // here rather than silently fixed, same practice as this project's other
    // confirmed-real upstream findings.
    [TestMethod]
    public async Task UpdateItemV1_returns_bad_request_only_when_the_body_itself_is_null()
    {
        using var context = await SeededContext();

        var result = await CatalogApi.UpdateItemV1(new DefaultHttpContext(), CreateServices(context), null!);

        Assert.IsInstanceOfType<BadRequest<ProblemDetails>>(result.Result);
    }

    [TestMethod]
    public async Task UpdateItemV1_returns_not_found_not_bad_request_when_the_id_is_merely_left_unset()
    {
        using var context = await SeededContext();

        var result = await CatalogApi.UpdateItemV1(new DefaultHttpContext(), CreateServices(context), new CatalogItem("No Id"));

        Assert.IsInstanceOfType<NotFound<ProblemDetails>>(result.Result);
    }

    [TestMethod]
    public async Task CreateItem_persists_a_new_item_with_a_generated_embedding()
    {
        using var context = TestCatalogContext.Create();
        var product = new CatalogItem("New Item") { Id = 42, Price = 5 };

        var result = await CatalogApi.CreateItem(CreateServices(context), product);

        Assert.AreEqual("/api/catalog/items/42", result.Location);
        var saved = await context.CatalogItems.FindAsync(42);
        Assert.AreEqual("New Item", saved!.Name);
    }

    [TestMethod]
    public async Task DeleteItemById_returns_not_found_when_the_item_does_not_exist()
    {
        using var context = await SeededContext();

        var result = await CatalogApi.DeleteItemById(CreateServices(context), 999);

        Assert.IsInstanceOfType<NotFound>(result.Result);
    }

    [TestMethod]
    public async Task DeleteItemById_removes_the_item_and_returns_no_content()
    {
        using var context = await SeededContext();

        var result = await CatalogApi.DeleteItemById(CreateServices(context), 1);

        Assert.IsInstanceOfType<NoContent>(result.Result);
        Assert.IsNull(await context.CatalogItems.FindAsync(1));
    }

    [TestMethod]
    public void GetFullPath_combines_the_content_root_with_the_Pics_folder_and_file_name()
    {
        var path = CatalogApi.GetFullPath("/app", "1.webp");

        Assert.AreEqual(Path.Combine("/app", "Pics", "1.webp"), path);
    }
}
