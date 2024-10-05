using MainCore.UI.ViewModels.Abstract;
using ReactiveUI;
using System.Reactive.Linq;

namespace MainCore.UI.ViewModels.UserControls
{
    [RegisterSingleton<IWaitingOverlayViewModel>]
    public class WaitingOverlayViewModel : ViewModelBase, IWaitingOverlayViewModel
    {
        public WaitingOverlayViewModel()
        {
            Message = "is initializing";
        }

        public async Task Show(string message)
        {
            await Observable.Start(() =>
            {
                Message = message;
                Shown = true;
            }, RxApp.MainThreadScheduler);
        }

        public async Task Show()
        {
            await Observable.Start(() =>
            {
                Shown = true;
            }, RxApp.MainThreadScheduler);
        }

        public async Task Hide()
        {
            await Observable.Start(() =>
            {
                Shown = false;
                Message = "is initializing";
            }, RxApp.MainThreadScheduler);
        }

        public async Task ChangeMessage(string message)
        {
            await Observable.Start(() =>
            {
                Message = message;
            }, RxApp.MainThreadScheduler);
        }

        private bool _shown;

        public bool Shown
        {
            get => _shown;
            set => this.RaiseAndSetIfChanged(ref _shown, value);
        }

        private string _message;

        public string Message
        {
            get => _message;
            set
            {
                var formattedValue = string.IsNullOrWhiteSpace(value) ? value : $"TBS is {value} ...";
                this.RaiseAndSetIfChanged(ref _message, formattedValue);
            }
        }
    }
}