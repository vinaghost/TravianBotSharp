using ReactiveUI;
using System.Reactive.Linq;

namespace MainCore.UI.ViewModels.Abstract
{
    public abstract class TabViewModelBase : ViewModelBase
    {
        private bool _isActive;

        protected readonly ReactiveCommand<bool, Unit> Command;

        protected TabViewModelBase()
        {
            Command = ReactiveCommand.CreateFromTask<bool>(Execute);

            this.WhenAnyValue(x => x.IsActive)
                .ObserveOn(RxApp.TaskpoolScheduler)
                .InvokeCommand(Command);
        }

        public bool IsActive
        {
            get => _isActive;
            set => this.RaiseAndSetIfChanged(ref _isActive, value);
        }

        private async Task Execute(bool isActive)
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