namespace MainCore.UI.ViewModels.Abstract
{
    public abstract partial class TabViewModelBase : ViewModelBase
    {
        [Reactive]
        private bool _isActive;

        protected TabViewModelBase()
        {
            this.WhenAnyValue(x => x.IsActive)
                .ObserveOn(RxApp.TaskpoolScheduler)
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