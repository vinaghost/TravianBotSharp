using MainCore.Tasks;

namespace MainCore.Commands.UI.Farming
{
    public class StartFarmListCommand : ByAccountIdBase, IRequest
    {
        public StartFarmListCommand(AccountId accountId) : base(accountId)
        {
        }
    }

    public class StartFarmListCommandHandler : IRequestHandler<StartFarmListCommand>
    {
        private readonly ITaskManager _taskManager;
        private readonly IDialogService _dialogService;
        private readonly IAccountSettingRepository _accountSettingRepository;
        private readonly IFarmRepository _farmRepository;

        public StartFarmListCommandHandler(ITaskManager taskManager, IDialogService dialogService, IAccountSettingRepository accountSettingRepository, IFarmRepository farmRepository)
        {
            _taskManager = taskManager;
            _dialogService = dialogService;
            _accountSettingRepository = accountSettingRepository;
            _farmRepository = farmRepository;
        }

        public async Task Handle(StartFarmListCommand request, CancellationToken cancellationToken)
        {
            var accountId = request.AccountId;
            var useStartAllButton = new GetAccountSetting().BooleanByName(accountId, Common.Enums.AccountSettingEnums.UseStartAllButton);
            if (!useStartAllButton)
            {
                var count = _farmRepository.CountActive(accountId);
                if (count == 0)
                {
                    _dialogService.ShowMessageBox("Information", "There is no active farm or use start all button is disable");
                    return;
                }
            }
            await _taskManager.AddOrUpdate<StartFarmListTask>(accountId);
            _dialogService.ShowMessageBox("Information", "Added start farm list task");
        }
    }
}