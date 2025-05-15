using MainCore.UI.Models.Output;
using Serilog;
using System.Diagnostics;
using System.Reactive.Concurrency;

namespace MainCore.UI
{
    [RegisterSingleton<ObservableExceptionHandler>]
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
        }

        private void Handle(Exception exception)
        {
            if (exception is null) return;
            Log.Error(exception, "UI execption");
            if (Debugger.IsAttached)
            {
                RxApp.MainThreadScheduler.Schedule(() => { throw exception; });
                Debugger.Break();
            }

            _dialogService.MessageBox.Handle(new MessageBoxData("Error", "There is something wrong. Please check logs/logs-Other.txt."))
                .Subscribe();
        }
    }
}