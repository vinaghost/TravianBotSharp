using MainCore.UI.ViewModels.Abstract;
using ReactiveUI;

namespace MainCore.UI.Models.Input
{
    public class CelebrationInput : ViewModelBase
    {
        private bool _great;

        public bool Great
        {
            get => _great;
            set => this.RaiseAndSetIfChanged(ref _great, value);
        }

        public bool Get()
        {
            return Great;
        }
    }
}