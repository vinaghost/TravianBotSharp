using MainCore.UI.Models.Output;
using System.Diagnostics;
using System.Reactive.Concurrency;

namespace MainCore.UI
{
    [RegisterSingleton<ObservableExceptionHandler>]
    public class ObservableExceptionHandler : IObserver<Exception>
    {
        private readonly IDialogService _dialogService;
        private readonly ILogger _logger;

        public ObservableExceptionHandler(IDialogService dialogService, ILogger logger)
        {
            _dialogService = dialogService;
            _logger = logger.ForContext<ObservableExceptionHandler>();
        }

        public void OnNext(Exception value)
        {
            Handle(value);
        }

        public void OnError(Exception error)
        {
            Handle(error);
        }

        public void OnCompleted()
        {
        }

        private void Handle(Exception exception)
        {
            if (exception is null) return;
            _logger.Error(exception, "UI execption");
            if (Debugger.IsAttached)
            {
                RxApp.MainThreadScheduler.Schedule(() => { throw exception; });
            }

            _dialogService.MessageBox.Handle(new MessageBoxData("Error", "There is something wrong. Please check logs/logs-Other.txt."))
                .Subscribe();
        }
    }
}