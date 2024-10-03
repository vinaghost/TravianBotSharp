namespace MainCore.Commands.Abstract
{
    public interface ICommand
    {
        Task<Result> Execute(CancellationToken cancellationToken);
    }

    public interface ICommand<in T>
    {
        Task<Result> Execute(T data, CancellationToken cancellationToken);
    }
}