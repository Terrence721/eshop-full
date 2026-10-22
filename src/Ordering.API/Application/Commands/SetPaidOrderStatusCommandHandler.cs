namespace eShop.Ordering.API.Application.Commands
{
    /// <summary>
    /// Handler moving an order to the paid status, plus its idempotent wrapper.
    /// </summary>
    // Regular CommandHandler
    public class SetPaidOrderStatusCommandHandler(IOrderRepository orderRepository) : IRequestHandler<SetPaidOrderStatusCommand, bool>
    {
        private readonly IOrderRepository _orderRepository = orderRepository ?? throw new ArgumentNullException(nameof(orderRepository));

        /// <summary>
        /// Handler which processes the command when
        /// Shipment service confirms the payment
        /// </summary>
        /// <param name="command"></param>
        /// <returns></returns>
        public async Task<bool> Handle(SetPaidOrderStatusCommand command, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(command);

            // Simulate a work time for validating the payment
            await Task.Delay(10000, cancellationToken);

            var orderToUpdate = await _orderRepository.GetAsync(command.OrderNumber);
            if (orderToUpdate == null)
            {
                return false;
            }

            orderToUpdate.SetPaidStatus();
            return await _orderRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken);
        }
    }


    // Use for Idempotency in Command process
    /// <summary>
    /// Identified (idempotent) wrapper around <see cref="SetPaidOrderStatusCommandHandler" />.
    /// </summary>
    public class SetPaidIdentifiedOrderStatusCommandHandler(
        IMediator mediator,
        IRequestManager requestManager,
        ILogger<IdentifiedCommandHandler<SetPaidOrderStatusCommand, bool>> logger) : IdentifiedCommandHandler<SetPaidOrderStatusCommand, bool>(mediator, requestManager, logger)
    {
        protected override bool CreateResultForDuplicateRequest()
        {
            return true; // Ignore duplicate requests for processing order.
        }
    }
}
