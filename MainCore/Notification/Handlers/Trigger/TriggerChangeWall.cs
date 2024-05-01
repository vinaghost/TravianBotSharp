﻿namespace MainCore.Notification.Handlers.Trigger
{
    public class TriggerChangeWall : INotificationHandler<VillageSettingUpdated>
    {
        private readonly UnitOfRepository _unitOfRepository;

        public TriggerChangeWall(UnitOfRepository unitOfRepository)
        {
            _unitOfRepository = unitOfRepository;
        }

        public async Task Handle(VillageSettingUpdated notification, CancellationToken cancellationToken)
        {
            await Task.CompletedTask;
            var villageId = notification.VillageId;
            _buildingRepository.UpdateWall(villageId);
        }
    }
}