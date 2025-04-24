using MainCore.UI.Models.Output;

namespace MainCore.Commands.UI
{
    [RegisterSingleton<PauseCommand>]
    public class PauseCommand
    {
        private readonly ITaskManager _taskManager;
        private readonly IDialogService _dialogService;
        private readonly ITimerManager _timeManager;
        private readonly StopCurrentTask.Handler _stopCurrentTask;

        public PauseCommand(ITaskManager taskManager, IDialogService dialogService, ITimerManager timeManager, StopCurrentTask.Handler stopCurrentTask)
        {
            _taskManager = taskManager;
            _dialogService = dialogService;
            _timeManager = timeManager;
            _stopCurrentTask = stopCurrentTask;
        }

        public async Task Execute(AccountId accountId, CancellationToken cancellationToken)
        {
            var status = _timeManager.GetStatus(accountId);
            if (status == StatusEnums.Paused)
            {
                await _timeManager.SetStatus(accountId, StatusEnums.Online);
                return;
            }

            if (status == StatusEnums.Online)
            {
                await _stopCurrentTask.HandleAsync(new(accountId), cancellationToken);
                return;
            }

            await _dialogService.MessageBox.Handle(new MessageBoxData("Information", $"Account is {status}"));
        }
    }
}