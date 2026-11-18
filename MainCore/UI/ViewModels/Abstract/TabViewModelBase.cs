namespace MainCore.UI.ViewModels.Abstract
{
    using ReactiveUI.Primitives;

    public abstract partial class TabViewModelBase : ViewModelBase
    {
        [Reactive]
        private bool _isActive;

        protected readonly DialogService _dialogService;

        protected TabViewModelBase(DialogService dialogService)
        {
            _dialogService = dialogService;
            Init();
        }

        private void Init()
        {
            this.WhenAnyValue(x => x.IsActive)
                .ObserveOn(RxSchedulers.TaskpoolScheduler)
                .InvokeCommand(ActivationCommand);
        }

        [ReactiveCommand]
        private async Task Activation(bool isActive)
        {
            if (isActive)
            {
                await OnActive();
            }
            else
            {
                await OnDeactive();
            }
        }

        protected virtual Task OnActive()
        {
            return Task.CompletedTask;
        }

        protected virtual Task OnDeactive()
        {
            return Task.CompletedTask;
        }
    }
}