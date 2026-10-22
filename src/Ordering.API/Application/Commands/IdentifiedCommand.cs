namespace eShop.Ordering.API.Application.Commands
{
    /// <summary>
    /// Wraps an inner command with a request id to provide idempotency (identified command).
    /// </summary>
    public class IdentifiedCommand<T, R>(T command, Guid id) : IRequest<R>
        where T : class, IRequest<R>
    {
        public T Command { get; } = command ?? throw new ArgumentNullException(nameof(command));
        public Guid Id { get; } = id;
    }
}