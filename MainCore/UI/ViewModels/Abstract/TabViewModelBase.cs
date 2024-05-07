using ReactiveUI;
using System.Reactive.Linq;

namespace MainCore.UI.ViewModels.Abstract
{
    public abstract class TabViewModelBase : ViewModelBase
    {
        private bool _isActive;
        private readonly ReactiveCommand<Unit, Unit> Active;
        private readonly ReactiveCommand<Unit, Unit> Deactive;

        public TabViewModelBase()
        {
            Active = ReactiveCommand.CreateFromTask(OnActive);
            Deactive = ReactiveCommand.CreateFromTask(OnDeactive);

            var isActiveObservable = this.WhenAnyValue(x => x.IsActive);

            isActiveObservable
                .Where(x => x == true)
                .Select(x => Unit.Default)
                .InvokeCommand(Active);

            isActiveObservable
                .Where(x => x == false)
                .Select(x => Unit.Default)
                .InvokeCommand(Deactive);
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