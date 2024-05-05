namespace MainCore.Commands.Base
{
    public interface ICommand : IRequest<Result>
    { }

    public interface ICommandHandler<in TCommand> : IRequestHandler<TCommand, Result>
        where TCommand : ICommand
    { }

    public interface ICommand<TResponse> : IRequest<Result<TResponse>>
    { }

    public interface ICommandHandler<in TCommand, TResponse> : IRequestHandler<TCommand, Result<TResponse>>
       where TCommand : ICommand<TResponse>
    { }
}