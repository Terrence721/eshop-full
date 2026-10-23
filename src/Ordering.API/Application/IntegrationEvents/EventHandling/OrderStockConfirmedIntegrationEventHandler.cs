namespace eShop.Ordering.API.Application.IntegrationEvents.EventHandling
{
    /// <summary>
    /// Handles stock-confirmed integration events by moving order status to stock-confirmed.
    /// </summary>
    public class OrderStockConfirmedIntegrationEventHandler(
        IMediator mediator,
        ILogger<OrderStockConfirmedIntegrationEventHandler> logger) :
        IIntegrationEventHandler<OrderStockConfirmedIntegrationEvent>
    {
        public async Task Handle(OrderStockConfirmedIntegrationEvent @event)
        {
            if (logger.IsEnabled(LogLevel.Information))
            {
                logger.LogInformation("Handling integration event: {IntegrationEventId} - ({@IntegrationEvent})", @event.Id, @event);
            }

            var command = new SetStockConfirmedOrderStatusCommand(@event.OrderId);

            if (logger.IsEnabled(LogLevel.Information))
            {
                logger.LogInformation(
                    "Sending command: {CommandName} - {IdProperty}: {CommandId} ({@Command})",
                    command.GetGenericTypeName(),
                    nameof(command.OrderNumber),
                    command.OrderNumber,
                    command);
            }

            await mediator.Send(command);
        }
    }
}
