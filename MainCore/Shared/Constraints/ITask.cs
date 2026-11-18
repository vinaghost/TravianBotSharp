namespace MainCore.Shared.Constraints;

public interface ITask : IConstraint
{
    DateTime ExecuteAt { get; set; }
    StageEnums Stage { get; set; }
    string Description { get; }
}