namespace eShop.Ordering.API.Application.Commands;

/// <summary>
/// Wraps an inner command with a request id to provide idempotency (identified command).
/// </summary>
public class IdentifiedCommand<T, R> : IRequest<R>
    where T : IRequest<R>
{
    public T Command { get; }
    public Guid Id { get; }
    public IdentifiedCommand(T command, Guid id)
    {
        Command = command;
        Id = id;
    }
}