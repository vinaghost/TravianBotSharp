using MainCore.Constraints;

namespace MainCore.Notifications.Handlers.Trigger
{
    [Handler]
    public static partial class NpcTaskTrigger
    {
        private static async ValueTask HandleAsync(
            IAccountVillageConstraint notification,
            ITaskManager taskManager,
            AppDbContext context,
            CancellationToken cancellationToken)
        {
            await Task.CompletedTask;
            var accountId = notification.AccountId;
            var villageId = notification.VillageId;

            var autoNPCEnable = context.BooleanByName(villageId, VillageSettingEnums.AutoNPCEnable);
            if (!autoNPCEnable)
            {
                taskManager.Remove<NpcTask.Task>(accountId, villageId);
                return;
            }

            var autoNPCGranaryPercent = context.ByName(villageId, VillageSettingEnums.AutoNPCGranaryPercent);
            var (crop, granary, production) = context.GetCropInfo(villageId);
            var villageName = context.GetVillageName(villageId);

            var tasks = taskManager
                .GetTaskList(accountId)
                .OfType<NpcTask.Task>()
                .Where(x => x.VillageId == villageId)
                .ToList();

            var executing = tasks.FirstOrDefault(x => x.Stage == StageEnums.Executing);
            var waiting = tasks.FirstOrDefault(x => x.Stage == StageEnums.Waiting);

            if (granary == 0)
            {
                if (executing is null && waiting is null)
                {
                    var npcTask = new NpcTask.Task(accountId, villageId, villageName)
                    {
                        ExecuteAt = DateTime.Now
                    };
                    taskManager.Add<NpcTask.Task>(npcTask);
                }
                return;
            }

            var currentPercent = crop * 100f / granary;

            DateTime? executeAt = null;

            if (currentPercent >= autoNPCGranaryPercent)
            {
                executeAt = DateTime.Now;
            }
            else if (production > 0)
            {
                var targetCrop = granary * autoNPCGranaryPercent / 100.0;
                var hours = (targetCrop - crop) / production;
                executeAt = DateTime.Now.AddHours(hours);
            }

            if (executeAt is null)
            {
                if (executing is null && waiting is null)
                {
                    var npcTask = new NpcTask.Task(accountId, villageId, villageName)
                    {
                        ExecuteAt = DateTime.Now
                    };
                    taskManager.Add<NpcTask.Task>(npcTask);
                }
                return;
            }

            if (executing is not null)
            {
                if (waiting is not null)
                {
                    waiting.ExecuteAt = executeAt.Value;
                    taskManager.ReOrder(accountId);
                }
                else
                {
                    var next = new NpcTask.Task(accountId, villageId, villageName)
                    {
                        ExecuteAt = executeAt.Value
                    };
                    taskManager.Add<NpcTask.Task>(next);
                }
            }
            else if (waiting is not null)
            {
                waiting.ExecuteAt = executeAt.Value;
                taskManager.ReOrder(accountId);
            }
            else
            {
                var npcTask = new NpcTask.Task(accountId, villageId, villageName)
                {
                    ExecuteAt = executeAt.Value
                };
                taskManager.Add<NpcTask.Task>(npcTask);
            }
        }

        private static int GetGranaryPercent(this AppDbContext context, VillageId villageId)
        {
            var percent = context.Storages
                .Where(x => x.VillageId == villageId.Value)
                .Select(x => x.Crop * 100f / x.Granary)
                .FirstOrDefault();
            return (int)percent;
        }

        private static (long Crop, long Granary, long Production) GetCropInfo(this AppDbContext context, VillageId villageId)
        {
            var data = context.Storages
                .Where(x => x.VillageId == villageId.Value)
                .Select(x => new { x.Crop, x.Granary, x.ProductionCrop })
                .FirstOrDefault();
            if (data is null) return (0, 0, 0);
            return (data.Crop, data.Granary, data.ProductionCrop);
        }
    }
}