using MainCore.UI.ViewModels.Abstract;
using Microsoft.Extensions.DependencyInjection;
using ReactiveUI.Primitives.Concurrency;
using ReactiveUI.Primitives.Disposables;

namespace MainCore.UI.ViewModels.UserControls
{
    [RegisterSingleton<IWaitingOverlayViewModel, WaitingOverlayViewModel>]
    public partial class WaitingOverlayViewModel : ViewModelBase, IWaitingOverlayViewModel
    {
        [RegisterServices]
        public static void Register(IServiceCollection services)
        {
            services
                .AddSingleton(x => (x.GetRequiredService<IWaitingOverlayViewModel>() as WaitingOverlayViewModel)!);
        }

        public async Task Show(string message)
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

        public async Task Show()
        {
            RxSchedulers.MainThreadScheduler.Schedule(
                state: this,
                action: static (sequencer, @this) =>
                {
                    @this.Shown = true;
                    return EmptyDisposable.Instance;
                });
        }

        public async Task Hide()
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

        public async Task ChangeMessage(string message)
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