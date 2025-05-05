namespace MainCore.Tasks.Base
{
    public interface IAccountTask : ITask
    {
        AccountId AccountId { get; }
    }
}