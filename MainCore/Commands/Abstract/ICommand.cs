namespace MainCore.Commands.Abstract
{
    public interface ICommand
    {
        Task<Result> Execute(CancellationToken cancellationToken);
    }

    public interface ICommand<out T> : ICommand
    {
        T Data { get; }
    }
}