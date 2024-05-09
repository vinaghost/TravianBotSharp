using ReactiveUI;
using System.Reactive.Linq;

namespace MainCore.UI.ViewModels.Abstract
{
    public abstract class TabViewModelBase : ViewModelBase
    {
        private bool _isActive;

        protected TabViewModelBase()
        {
            var isActiveObservable = this.WhenAnyValue(x => x.IsActive);

            isActiveObservable
                .Where(active => active)
                .Select(x => Unit.Default)
                .InvokeCommand(ReactiveCommand.CreateFromTask(OnActive));

            isActiveObservable
                .Where(active => !active)
                .Select(x => Unit.Default)
                .InvokeCommand(ReactiveCommand.CreateFromTask(OnDeactive));
        }

        public bool IsActive
        {
            get => _isActive;
            set => this.RaiseAndSetIfChanged(ref _isActive, value);
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