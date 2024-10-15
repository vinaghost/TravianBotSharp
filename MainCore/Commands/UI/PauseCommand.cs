using MainCore.UI.Models.Output;
using System.Reactive.Linq;

namespace MainCore.Commands.UI
{
    [RegisterSingleton<PauseCommand>]
    public class PauseCommand
    {
        private readonly ITaskManager _taskManager;
        private readonly IDialogService _dialogService;

        public PauseCommand(ITaskManager taskManager, IDialogService dialogService)
        {
            _taskManager = taskManager;
            _dialogService = dialogService;
        }

        public async Task Execute(AccountId accountId, CancellationToken cancellationToken)
        {
            var status = _taskManager.GetStatus(accountId);
            if (status == StatusEnums.Paused)
            {
                await _taskManager.SetStatus(accountId, StatusEnums.Online);
                return;
            }

            if (status == StatusEnums.Online)
            {
                await _taskManager.StopCurrentTask(accountId);
                await _taskManager.SetStatus(accountId, StatusEnums.Paused);
                return;
            }

            await _dialogService.MessageBox.Handle(new MessageBoxData("Information", $"Account is {status}"));
        }
    }
}