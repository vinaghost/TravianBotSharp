using MainCore.Tasks.Constraints;

namespace MainCore.Tasks.Base
{
    public abstract class TaskBase(DateTime executeAt) : ITask
    {
        public StageEnums Stage { get; set; } = StageEnums.Waiting;
        public DateTime ExecuteAt { get; set; } = executeAt;
        public virtual string Description { get; }
        protected virtual string TaskName { get; }
    }
}