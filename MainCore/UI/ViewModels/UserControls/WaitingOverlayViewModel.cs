using MainCore.UI.ViewModels.Abstract;

using ReactiveUI.Primitives.Concurrency;
using ReactiveUI.Primitives.Disposables;

namespace MainCore.UI.ViewModels.UserControls
{
    [RegisterSingleton<WaitingOverlayViewModel>]
    public partial class WaitingOverlayViewModel : ViewModelBase
    {
        public void Show(string message)
        {
            RxSchedulers.MainThreadScheduler.Schedule(
                state: (this, message),
                action: static (sequencer, state) =>
                {
                    var (@this, msg) = state;
                    @this.Message = msg;
                    return EmptyDisposable.Instance;
                });
        }

        public void Show()
        {
            RxSchedulers.MainThreadScheduler.Schedule(
                state: this,
                action: static (sequencer, @this) =>
                {
                    @this.Shown = true;
                    return EmptyDisposable.Instance;
                });
        }

        public void Hide()
        {
            RxSchedulers.MainThreadScheduler.Schedule(
                state: this,
                action: static (sequencer, @this) =>
                {
                    @this.Shown = false;
                    @this.Message = "is initializing";
                    return EmptyDisposable.Instance;
                });
        }

        public void ChangeMessage(string message)
        {
            RxSchedulers.MainThreadScheduler.Schedule(
                state: (this, message),
                action: static (sequencer, state) =>
                {
                    var (@this, msg) = state;
                    @this.Message = msg;
                    return EmptyDisposable.Instance;
                });
        }

        [Reactive]
        private bool _shown;

        private string _message = "TBS is initializing";

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