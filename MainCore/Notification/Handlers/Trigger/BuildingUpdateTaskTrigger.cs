using MainCore.Constraints;

namespace MainCore.Notification.Handlers.Trigger
{
    [Handler]
    public static partial class BuildingUpdateTaskTrigger
    {
        private static async ValueTask HandleAsync(
            IAccountConstraint notification,
            GetVillageNameQuery.Handler getVillageNameQuery,
            ITaskManager taskManager,
            ISettingService settingService,
            GetMissingBuildingVillagesQuery.Handler getMissingBuildingVillageQuery,
            CancellationToken cancellationToken)
        {
            var accountId = notification.AccountId;

            var autoLoadVillageBuilding = settingService.BooleanByName(accountId, AccountSettingEnums.EnableAutoLoadVillageBuilding);
            if (!autoLoadVillageBuilding) return;

            var villages = await getMissingBuildingVillageQuery.HandleAsync(new(accountId));
            foreach (var village in villages)
            {
                var villageName = await getVillageNameQuery.HandleAsync(new(village), cancellationToken);
                await taskManager.AddOrUpdate<UpdateBuildingTask.Task>(new(accountId, village, villageName));
            }
        }
    }
}