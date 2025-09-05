using MainCore.Tasks.Base;

namespace MainCore.UI.Models.Output
{
    public partial class TaskItem : ReactiveObject
    {
        public TaskItem(BaseTask task)
        {
            Task = task.Description;
            ExecuteAt = task.ExecuteAt;
            Stage = task.Stage;
        }

        [Reactive]
        private string _task = "";

        [Reactive]
        private DateTime _executeAt = DateTime.MinValue;

        [Reactive]
        private StageEnums _stage = StageEnums.Waiting;

        public void CopyFrom(TaskItem taskItem)
        {
            Task = taskItem.Task;
            ExecuteAt = taskItem.ExecuteAt;
            Stage = taskItem.Stage;
        }
    }
}
