namespace MainCore.Tasks.Base
{
    public interface ITask
    {
        DateTime ExecuteAt { get; set; }
        StageEnums Stage { get; set; }
        string Description { get; }
    }
}