namespace eShop.Ordering.API.Application.IntegrationEvents.EventHandling
{
    /// <summary>
    /// Handles a payment-failed integration event by cancelling the associated order.
    /// </summary>
    public class OrderPaymentFailedIntegrationEventHandler(
        IMediator mediator,
        ILogger<OrderPaymentFailedIntegrationEventHandler> logger) :
        IIntegrationEventHandler<OrderPaymentFailedIntegrationEvent>
    {
        public async Task Handle(OrderPaymentFailedIntegrationEvent @event)
        {
            if (logger.IsEnabled(LogLevel.Information))
            {
                logger.LogInformation("Handling integration event: {IntegrationEventId} - ({@IntegrationEvent})", @event.Id, @event);
            }

            var command = new CancelOrderCommand(@event.OrderId);

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
