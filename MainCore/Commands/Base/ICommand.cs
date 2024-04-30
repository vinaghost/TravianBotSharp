namespace MainCore.Commands.Base
{
    public interface ICommand : IRequest<Result>
    { }

    public interface ICommand<TResponse> : IRequest<Result>
    { }

    public interface ICommandHandler<in TCommand> : IRequestHandler<TCommand, Result>
        where TCommand : ICommand
    { }

    public interface ICommandHandler<in TCommand, TResponse> : IRequestHandler<TCommand, Result>
       where TCommand : ICommand<TResponse>
    {
        TResponse Value { get; }
    }
}