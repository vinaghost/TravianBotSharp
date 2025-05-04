using MainCore.Tasks.Base;

namespace MainCore.UI.Models.Output
{
    public class TaskItem
    {
        public TaskItem(TaskBase task)
        {
            Task = task.Description;
            ExecuteAt = task.ExecuteAt;
            Stage = task.Stage;
        }

        public string Task { get; set; }
        public DateTime ExecuteAt { get; set; }
        public StageEnums Stage { get; set; }
    }
}