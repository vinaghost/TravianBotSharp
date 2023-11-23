using MainCore.Common.Enums;
using MainCore.Tasks.Base;
using System;

namespace MainCore.UI.Models.Output
{
    public class TaskItem
    {
        public TaskItem(TaskBase task)
        {
            Task = task.GetName();
            ExecuteAt = task.ExecuteAt;
            Stage = task.Stage;
        }

        public string Task { get; set; }
        public DateTime ExecuteAt { get; set; }
        public StageEnums Stage { get; set; }
    }
}