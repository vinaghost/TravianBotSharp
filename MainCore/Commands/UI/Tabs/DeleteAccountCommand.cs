namespace MainCore.Commands.UI.Tabs
{
    public class DeleteAccountCommand
    {
        private readonly ITaskManager _taskManager;
        private readonly IDialogService _dialogService;
        private readonly IMediator _mediator;
        private readonly IDbContextFactory<AppDbContext> _contextFactory;

        public DeleteAccountCommand(ITaskManager taskManager, IDialogService dialogService, IMediator mediator, IDbContextFactory<AppDbContext> contextFactory)
        {
            _taskManager = taskManager;
            _dialogService = dialogService;
            _mediator = mediator;
            _contextFactory = contextFactory;
        }

        public async Task Execute(AccountId accountId, CancellationToken cancellationToken)
        {
            var status = _taskManager.GetStatus(accountId);
            if (status != StatusEnums.Offline)
            {
                _dialogService.ShowMessageBox("Warning", "Account should be offline");
                return;
            }

            DeleteAccountFromDatabase(accountId);
            await _mediator.Publish(new AccountUpdated(), cancellationToken);
        }

        private void DeleteAccountFromDatabase(AccountId accountId)
        {
            using var context = _contextFactory.CreateDbContext();
            context.Accounts
                .Where(x => x.Id == accountId.Value)
                .ExecuteDelete();
        }
    }
}