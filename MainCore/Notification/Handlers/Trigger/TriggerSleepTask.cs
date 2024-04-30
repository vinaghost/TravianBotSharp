using MainCore.Tasks;

namespace MainCore.Notification.Handlers.Trigger
{
    public class TriggerSleepTask : INotificationHandler<AccountInit>
    {
        private readonly ITaskManager _taskManager;
        private readonly UnitOfRepository _unitOfRepository;

        public TriggerSleepTask(ITaskManager taskManager, UnitOfRepository unitOfRepository)
        {
            _taskManager = taskManager;
            _unitOfRepository = unitOfRepository;
        }

        public async Task Handle(AccountInit notification, CancellationToken cancellationToken)
        {
            var accountId = notification.AccountId;
            await Trigger(accountId);
        }

        private async Task Trigger(AccountId accountId)
        {
            if (_taskManager.IsExist<SleepTask>(accountId)) return;
            var workTime = _unitOfRepository.AccountSettingRepository.GetByName(accountId, AccountSettingEnums.WorkTimeMin, AccountSettingEnums.WorkTimeMax);
            await _taskManager.Add<SleepTask>(accountId, executeTime: DateTime.Now.AddMinutes(workTime));
        }
    }
}