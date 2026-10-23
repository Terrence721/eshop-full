namespace eShop.Ordering.API.Application.Behaviors
{
    /// <summary>
    /// MediatR pipeline behavior running all FluentValidation validators for a request.
    /// </summary>
    public class ValidatorBehavior<TRequest, TResponse>(IEnumerable<IValidator<TRequest>> validators, ILogger<ValidatorBehavior<TRequest, TResponse>> logger) : IPipelineBehavior<TRequest, TResponse> where TRequest : IRequest<TResponse>
    {
        private readonly ILogger<ValidatorBehavior<TRequest, TResponse>> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        private readonly IEnumerable<IValidator<TRequest>> _validators = validators ?? throw new ArgumentNullException(nameof(validators));

        public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
        {
            var typeName = request.GetGenericTypeName();

            if (_logger.IsEnabled(LogLevel.Information))
            {
                _logger.LogInformation("Validating command {CommandType}", typeName);
            }

            var validationTasks = _validators.Select(v => v.ValidateAsync(request, cancellationToken));
            var validationResults = await Task.WhenAll(validationTasks);

            var failures = validationResults
                .SelectMany(result => result.Errors)
                .Where(error => error != null)
                .ToList();

            if (failures.Count > 0)
            {
                if (_logger.IsEnabled(LogLevel.Warning))
                {
                    _logger.LogWarning("Validation errors - {CommandType} - Command: {@Command} - Errors: {@ValidationErrors}", typeName, request, failures);
                }

                throw new OrderingDomainException(
                    $"Command Validation Errors for type {typeof(TRequest).Name}", new ValidationException("Validation exception", failures));
            }

            return await next(cancellationToken);
        }
    }
}
