using MainCore.UI.Models.Output;

namespace MainCore.Commands.UI
{
    [RegisterSingleton<DeleteAccountCommand>]
    public class DeleteAccountCommand
    {
        private readonly ITaskManager _taskManager;
        private readonly IDialogService _dialogService;
        private readonly IDbContextFactory<AppDbContext> _contextFactory;
        private readonly AccountUpdated.Handler _accountUpdated;

        public DeleteAccountCommand(ITaskManager taskManager, IDialogService dialogService, IDbContextFactory<AppDbContext> contextFactory, AccountUpdated.Handler accountUpdated)
        {
            _taskManager = taskManager;
            _dialogService = dialogService;
            _contextFactory = contextFactory;
            _accountUpdated = accountUpdated;
        }

        public async Task Execute(AccountId accountId, CancellationToken cancellationToken)
        {
            var status = _taskManager.GetStatus(accountId);
            if (status != StatusEnums.Offline)
            {
                await _dialogService.MessageBox.Handle(new MessageBoxData("Warning", "Account should be offline"));
                return;
            }

            DeleteAccountFromDatabase(accountId);
            await _accountUpdated.HandleAsync(new(), cancellationToken);
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