using MainCore.Constraints;
using MainCore.Specifications;

namespace MainCore.Notifications.Trigger
{
    [Handler]
    public static partial class UpdateVillageTaskTrigger
    {
        private static async ValueTask HandleAsync(
            IAccountVillageConstraint notification,
            ITaskManager taskManager,
            AppDbContext context,
            CancellationToken cancellationToken)
        {
            await Task.CompletedTask;
            var (accountId, villageId) = notification;

            var taskExist = taskManager.IsExist<UpdateVillageTask.Task>(accountId, villageId);
            if (taskExist) return;

            var settingEnable = context.BooleanByName(villageId, VillageSettingEnums.AutoRefreshEnable);
            if (!settingEnable) return;

            var getVillageSpec = new GetVillageNameSpec(villageId);
            var villageName = context.Villages
                .WithSpecification(getVillageSpec)
                .First();
            taskManager.Add<UpdateVillageTask.Task>(new(accountId, villageId, villageName));
        }
    }
}