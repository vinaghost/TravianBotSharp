namespace MainCore.Notification.Handlers.Trigger
{
    public class TriggerChangeWall : INotificationHandler<VillageSettingUpdated>
    {
        private readonly IDbContextFactory<AppDbContext> _contextFactory;

        public TriggerChangeWall(IDbContextFactory<AppDbContext> contextFactory)
        {
            _contextFactory = contextFactory;
        }

        public async Task Handle(VillageSettingUpdated notification, CancellationToken cancellationToken)
        {
            await Task.CompletedTask;
            var villageId = notification.VillageId;
            UpdateWall(villageId);
        }

        public void UpdateWall(VillageId villageId)
        {
            using var context = _contextFactory.CreateDbContext();
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