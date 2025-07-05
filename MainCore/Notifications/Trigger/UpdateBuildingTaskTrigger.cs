using MainCore.Constraints;

namespace MainCore.Notifications.Trigger
{
    [Handler]
    public static partial class UpdateBuildingTaskTrigger
    {
        private static async ValueTask HandleAsync(
            IAccountConstraint notification,
            ITaskManager taskManager,
            GetMissingBuildingVillagesQuery.Handler getMissingBuildingVillageQuery,
            AppDbContext context,
            CancellationToken cancellationToken)
        {
            var accountId = notification.AccountId;

            var settingEnable = context.BooleanByName(accountId, AccountSettingEnums.EnableAutoLoadVillageBuilding);
            if (!settingEnable) return;

            var villages = await getMissingBuildingVillageQuery.HandleAsync(new(accountId));
            foreach (var village in villages)
            {
                var villageName = context.GetVillageName(village);
                taskManager.AddOrUpdate<UpdateBuildingTask.Task>(new(accountId, village, villageName));
            }
        }
    }
}