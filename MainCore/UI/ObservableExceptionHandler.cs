using ReactiveUI;
using Serilog;
using System.Diagnostics;
using System.Reactive.Concurrency;

namespace MainCore.UI
{
    [RegisterSingleton]
    public class ObservableExceptionHandler : IObserver<Exception>
    {
        private readonly IDialogService _dialogService;

        public ObservableExceptionHandler(IDialogService dialogService)
        {
            _dialogService = dialogService;
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
            Handle(null);
        }

        private void Handle(Exception exception)
        {
            if (exception is null) return;
            Log.Error(exception, "UI execption");
            if (Debugger.IsAttached)
            {
                Debugger.Break();
                RxApp.MainThreadScheduler.Schedule(() => { throw exception; });
            }

            _dialogService.ShowMessageBox("Error", "There is something wrong. Please check logs/logs-Other.txt.");
        }
    }
}