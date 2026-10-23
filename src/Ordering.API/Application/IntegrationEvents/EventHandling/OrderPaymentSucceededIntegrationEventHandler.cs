namespace eShop.Ordering.API.Application.IntegrationEvents.EventHandling
{
    /// <summary>
    /// Handles a payment-succeeded integration event by setting the associated order to paid status.
    /// </summary>
    public class OrderPaymentSucceededIntegrationEventHandler(
        IMediator mediator,
        ILogger<OrderPaymentSucceededIntegrationEventHandler> logger) :
        IIntegrationEventHandler<OrderPaymentSucceededIntegrationEvent>
    {
        public async Task Handle(OrderPaymentSucceededIntegrationEvent @event)
        {
            if (logger.IsEnabled(LogLevel.Information))
            {
                logger.LogInformation("Handling integration event: {IntegrationEventId} - ({@IntegrationEvent})", @event.Id, @event);
            }

            var command = new SetPaidOrderStatusCommand(@event.OrderId);

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
