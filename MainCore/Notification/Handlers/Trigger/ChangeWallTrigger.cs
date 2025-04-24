namespace MainCore.Notification.Handlers.Trigger
{
    [Handler]
    public static partial class ChangeWallTrigger
    {
        private static async ValueTask HandleAsync(
            ByAccountVillageIdBase notification,
            IDbContextFactory<AppDbContext> contextFactory,
            CancellationToken cancellationToken)
        {
            var villageId = notification.VillageId;
            using var context = await contextFactory.CreateDbContextAsync(cancellationToken);
            var wallBuilding = context.Buildings
                .Where(x => x.VillageId == villageId.Value)
                .Where(x => x.Location == 40)
                .FirstOrDefault();
            if (wallBuilding is null) return;

            var tribe = (TribeEnums)context.VillagesSetting
                .Where(x => x.VillageId == villageId.Value)
                .Where(x => x.Setting == VillageSettingEnums.Tribe)
                .Select(x => x.Value)
                .FirstOrDefault();

            var wall = tribe.GetWall();
            if (wallBuilding.Type == wall) return;

            wallBuilding.Type = wall;
            context.Update(wallBuilding);
            context.SaveChanges();
        }
    }
}