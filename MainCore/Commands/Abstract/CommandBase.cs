namespace MainCore.Commands.Abstract
{
    public abstract class CommandBase(IDataService dataService)
    {
        protected IDataService _dataService = dataService;
    }
}