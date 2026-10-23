namespace eShop.Ordering.API.Application.Behaviors
{
    using Microsoft.Extensions.Logging;

    public class TransactionBehavior<TRequest, TResponse>(OrderingContext dbContext,
        IOrderingIntegrationEventService orderingIntegrationEventService,
        ILogger<TransactionBehavior<TRequest, TResponse>> logger) : IPipelineBehavior<TRequest, TResponse> where TRequest : IRequest<TResponse>
    {
        private readonly ILogger<TransactionBehavior<TRequest, TResponse>> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        private readonly OrderingContext _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        private readonly IOrderingIntegrationEventService _orderingIntegrationEventService = orderingIntegrationEventService ?? throw new ArgumentNullException(nameof(orderingIntegrationEventService));

        public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
        {
            var response = default(TResponse);
            var typeName = request.GetGenericTypeName();

            try
            {
                if (_dbContext.HasActiveTransaction)
                {
                    return await next(cancellationToken);
                }

                var strategy = _dbContext.Database.CreateExecutionStrategy();

                await strategy.ExecuteAsync(async () =>
                {
                    Guid transactionId;

                    // Guarded by the HasActiveTransaction check above: BeginTransactionAsync only
                    // returns null when a transaction is already active, which can't be the case here.
                    await using var transaction = (await _dbContext.BeginTransactionAsync())!;
                    using (_logger.BeginScope(new List<KeyValuePair<string, object>> { new("TransactionContext", transaction.TransactionId) }))
                    {
                        if (_logger.IsEnabled(LogLevel.Information))
                        {
                            _logger.LogInformation("Begin transaction {TransactionId} for {CommandName} ({@Command})", transaction.TransactionId, typeName, request);
                        }

                        response = await next(cancellationToken);

                        if (_logger.IsEnabled(LogLevel.Information))
                        {
                            _logger.LogInformation("Commit transaction {TransactionId} for {CommandName}", transaction.TransactionId, typeName);
                        }

                        await _dbContext.CommitTransactionAsync(transaction);

                        transactionId = transaction.TransactionId;
                    }

                    await _orderingIntegrationEventService.PublishEventsThroughEventBusAsync(transactionId);
                });

                // response is always set by strategy.ExecuteAsync's lambda on the success path --
                // any failure inside it propagates as an exception to the catch block below instead,
                // which rethrows without reaching this line. The compiler can't see across that
                // lambda boundary for an unconstrained TResponse.
                return response!;
            }
            catch (Exception ex)
            {
                if (_logger.IsEnabled(LogLevel.Error))
                {
                    _logger.LogError(ex, "Error Handling transaction for {CommandName} ({@Command})", typeName, request);
                }

                throw;
            }
        }
    }
}
