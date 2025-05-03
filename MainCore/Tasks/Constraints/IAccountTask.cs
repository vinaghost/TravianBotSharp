namespace MainCore.Tasks.Constraints
{
    public interface IAccountTask : ITask
    {
        AccountId AccountId { get; }
    }
}