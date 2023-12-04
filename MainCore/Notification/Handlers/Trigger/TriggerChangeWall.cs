using MainCore.Notification.Message;
using MainCore.Repositories;
using MediatR;

namespace MainCore.Notification.Handlers.Trigger
{
    public class TriggerChangeWall : INotificationHandler<VillageSettingUpdated>
    {
        private readonly IUnitOfRepository _unitOfRepository;

        public TriggerChangeWall(IUnitOfRepository unitOfRepository)
        {
            _unitOfRepository = unitOfRepository;
        }

        public async Task Handle(VillageSettingUpdated notification, CancellationToken cancellationToken)
        {
            await Task.CompletedTask;
            var villageId = notification.VillageId;
            _unitOfRepository.BuildingRepository.UpdateWall(villageId);
        }
    }
}