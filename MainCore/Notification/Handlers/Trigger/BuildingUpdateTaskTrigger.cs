using MainCore.Constraints;

namespace MainCore.Notification.Handlers.Trigger
{
    [Handler]
    public static partial class BuildingUpdateTaskTrigger
    {
        private static async ValueTask HandleAsync(
            IAccountNotification notification,
            GetVillageNameQuery.Handler getVillageNameQuery,
            ITaskManager taskManager,
            AppDbContext context,
            GetMissingBuildingVillagesQuery.Handler getMissingBuildingVillageQuery,
            CancellationToken cancellationToken)
        {
            var accountId = notification.AccountId;

            var autoLoadVillageBuilding = context.BooleanByName(accountId, AccountSettingEnums.EnableAutoLoadVillageBuilding);
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