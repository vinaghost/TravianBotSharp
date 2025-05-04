using MainCore.Tasks.Constraints;

namespace MainCore.Tasks.Base
{
    public abstract class TaskBase : ITask
    {
        public virtual string Key { get; }
        public StageEnums Stage { get; set; } = StageEnums.Waiting;
        public DateTime ExecuteAt { get; set; } = DateTime.Now;
        public virtual string Description { get; }
        protected virtual string TaskName { get; }
    }
}