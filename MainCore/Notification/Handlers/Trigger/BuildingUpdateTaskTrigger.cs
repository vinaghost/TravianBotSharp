using MainCore.Tasks;

namespace MainCore.Notification.Handlers.Trigger
{
    [Handler]
    public static partial class BuildingUpdateTaskTrigger
    {
        private static async ValueTask HandleAsync(
            ByAccountIdBase notification,
            ITaskManager taskManager,
            IDbContextFactory<AppDbContext> contextFactory,
            GetMissingBuildingVillagesQuery.Handler getMissingBuildingVillageQuery,
            CancellationToken cancellationToken)
        {
            var accountId = notification.AccountId;
            using var context = await contextFactory.CreateDbContextAsync(cancellationToken);
            var autoLoadVillageBuilding = context.BooleanByName(accountId, AccountSettingEnums.EnableAutoLoadVillageBuilding);
            if (!autoLoadVillageBuilding) return;

            var villages = await getMissingBuildingVillageQuery.HandleAsync(new(accountId));

            foreach (var village in villages)
            {
                await taskManager.AddOrUpdate<UpdateBuildingTask>(accountId, village);
            }
        }
    }
}