namespace eShop.Ordering.API.Application.Validations
{
    /// <summary>
    /// Validates a <see cref="CreateOrderCommand" /> (address, card details, and order items).
    /// </summary>
    public class CreateOrderCommandValidator : AbstractValidator<CreateOrderCommand>
    {
        public CreateOrderCommandValidator(ILogger<CreateOrderCommandValidator> logger)
        {
            RuleFor(command => command.UserId).NotEmpty();
            RuleFor(command => command.UserName).NotEmpty();
            RuleFor(command => command.City).NotEmpty();
            RuleFor(command => command.Street).NotEmpty();
            RuleFor(command => command.State).NotEmpty();
            RuleFor(command => command.Country).NotEmpty();
            RuleFor(command => command.ZipCode).NotEmpty();
            RuleFor(command => command.CardNumber).NotEmpty().Length(12, 19);
            RuleFor(command => command.CardHolderName).NotEmpty();
            RuleFor(command => command.CardExpiration).NotEmpty().Must(BeValidExpirationDate).WithMessage("Please specify a valid card expiration date");
            RuleFor(command => command.CardSecurityNumber).NotEmpty().Length(3);
            RuleFor(command => command.CardTypeId).NotEmpty();
            RuleFor(command => command.OrderItems).Must(ContainOrderItems).WithMessage("No order items found");

            if (logger.IsEnabled(LogLevel.Trace))
            {
                logger.LogTrace("INSTANCE CREATED - {ClassName}", GetType().Name);
            }
        }

        private bool BeValidExpirationDate(DateTime dateTime)
        {
            return dateTime >= DateTime.UtcNow;
        }

        private bool ContainOrderItems(IEnumerable<OrderItemDTO>? orderItems)
        {
            return orderItems is not null && orderItems.Count() > 0;
        }
    }
}
