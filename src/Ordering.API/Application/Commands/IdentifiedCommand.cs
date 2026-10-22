namespace eShop.Ordering.API.Application.Commands;

/// <summary>
/// Wraps an inner command with a request id to provide idempotency (identified command).
/// </summary>
public class IdentifiedCommand<T, R>(T command, Guid id) : IRequest<R>
    where T : IRequest<R>
{
    public T Command { get; } = command;
    public Guid Id { get; } = id;
}
