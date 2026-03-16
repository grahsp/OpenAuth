namespace OpenAuth.Application.Abstractions;

public interface ICommandHandler<in TCommand> : IHandler where TCommand : ICommand
{
	Task HandleAsync(TCommand command, CancellationToken ct);
}

public interface ICommandHandler<in TCommand, TResult> : IHandler where TCommand : ICommand<TResult>
{
	Task<TResult> HandleAsync(TCommand command, CancellationToken ct);
}
