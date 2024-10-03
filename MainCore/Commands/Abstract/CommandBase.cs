namespace MainCore.Commands.Abstract
{
    public abstract class CommandBase(DataService dataService)
    {
        protected DataService _dataService = dataService;
    }
}