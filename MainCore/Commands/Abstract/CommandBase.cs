namespace MainCore.Commands.Abstract
{
    public abstract class CommandBase(DataService dataService) : ICommand
    {
        protected DataService _dataService = dataService;

        public abstract Task<Result> Execute(CancellationToken cancellationToken);
    }

    public abstract class CommandBase<T>(DataService dataService) : CommandBase(dataService), ICommand<T>
    {
        public T Data { get; protected set; }

        public Task<Result> Execute(T data, CancellationToken cancellationToken)
        {
            Data = data;
            return Execute(cancellationToken);
        }
    }
}