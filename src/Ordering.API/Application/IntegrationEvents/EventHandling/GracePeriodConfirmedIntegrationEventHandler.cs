namespace eShop.Ordering.API.Application.IntegrationEvents.EventHandling
{
    /// <summary>
    /// Handles the confirmed grace period by moving the order to awaiting validation.
    /// </summary>
    public class GracePeriodConfirmedIntegrationEventHandler(
        IMediator mediator,
        ILogger<GracePeriodConfirmedIntegrationEventHandler> logger) : IIntegrationEventHandler<GracePeriodConfirmedIntegrationEvent>
    {
        /// <summary>
        /// Event handler which confirms that the grace period
        /// has been completed and order will not initially be cancelled.
        /// Therefore, the order process continues for validation.
        /// </summary>
        /// <param name="event">
        /// </param>
        /// <returns></returns>
        public async Task Handle(GracePeriodConfirmedIntegrationEvent @event)
        {
            if (logger.IsEnabled(LogLevel.Information))
            {
                logger.LogInformation("Handling integration event: {IntegrationEventId} - ({@IntegrationEvent})", @event.Id, @event);
            }

            var command = new SetAwaitingValidationOrderStatusCommand(@event.OrderId);

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
