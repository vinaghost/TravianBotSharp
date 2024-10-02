namespace MainCore.Commands.Abstract
{
    public abstract class CommandBase(DataService dataService)
    {
        protected DataService _dataService = dataService;
    }

    public interface ICommand
    {
        Task<Result> Execute(CancellationToken cancellationToken);
    }

    public interface ICommand<in T>
    {
        Task<Result> Execute(T data, CancellationToken cancellationToken);
    }
}