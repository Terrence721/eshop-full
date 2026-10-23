namespace eShop.Ordering.API.Application.Behaviors
{
    /// <summary>
    /// MediatR pipeline behavior that logs the request and its response around handler execution.
    /// </summary>
    public class LoggingBehavior<TRequest, TResponse>(ILogger<LoggingBehavior<TRequest, TResponse>> logger) : IPipelineBehavior<TRequest, TResponse> where TRequest : IRequest<TResponse>
    {
        private readonly ILogger<LoggingBehavior<TRequest, TResponse>> _logger = logger;

        public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
        {
            if (_logger.IsEnabled(LogLevel.Information))
            {
                _logger.LogInformation("Handling command {CommandName} ({@Command})", request.GetGenericTypeName(), request);
            }
            var response = await next(cancellationToken);
            if (_logger.IsEnabled(LogLevel.Information))
            {
                _logger.LogInformation("Command {CommandName} handled - response: {@Response}", request.GetGenericTypeName(), response);
            }

            return response;
        }
    }
}
