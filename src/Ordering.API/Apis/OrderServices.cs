/// <summary>
/// Lightweight parameter object bundling the cross-cutting services required by
/// the ordering endpoint handlers (mediator, queries, identity, logger).
/// </summary>
namespace eShop.Ordering.API.Apis
{
    public class OrderServices(
    IMediator mediator,
    IOrderQueries queries,
    IIdentityService identityService,
    ILogger<OrderServices> logger)
{
    public IMediator Mediator { get; set; } = mediator;
    public ILogger<OrderServices> Logger { get; } = logger;
    public IOrderQueries Queries { get; } = queries;
    public IIdentityService IdentityService { get; } = identityService;
}
}
