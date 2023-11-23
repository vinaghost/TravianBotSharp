using ReactiveUI;
using System.Reactive;

namespace MainCore.UI.ViewModels.Abstract
{
    public abstract class TabViewModelBase : ViewModelBase
    {
        private bool _isActive;
        private readonly ReactiveCommand<bool, Unit> TabStatusChanged;

        public TabViewModelBase()
        {
            TabStatusChanged = ReactiveCommand.CreateFromTask<bool>(TabStatusChangedHandler);

            this.WhenAnyValue(x => x.IsActive)
                .InvokeCommand(TabStatusChanged);
        }

        public bool IsActive
        {
            get => _isActive;
            set => this.RaiseAndSetIfChanged(ref _isActive, value);
        }

        private async Task TabStatusChangedHandler(bool isActive)
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