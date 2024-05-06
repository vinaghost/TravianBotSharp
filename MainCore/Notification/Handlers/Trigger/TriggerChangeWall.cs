namespace MainCore.Notification.Handlers.Trigger
{
    public class TriggerChangeWall : INotificationHandler<VillageSettingUpdated>
    {
        private readonly IBuildingRepository _buildingRepository;

        public TriggerChangeWall(IBuildingRepository buildingRepository)
        {
            _buildingRepository = buildingRepository;
        }

        public async Task Handle(VillageSettingUpdated notification, CancellationToken cancellationToken)
        {
            await Task.CompletedTask;
            var villageId = notification.VillageId;
            _buildingRepository.UpdateWall(villageId);
        }
    }
}