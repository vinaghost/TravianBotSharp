using MainCore.UI.ViewModels.Abstract;
using ReactiveUI;

namespace MainCore.UI.ViewModels.UserControls
{
    public class AmountInputViewModel : ViewModelBase
    {
        public int Get() => Value;

        public void Set(int value) => Value = value;

        private int _value;

        public int Value
        {
            get => _value;
            set => this.RaiseAndSetIfChanged(ref _value, value);
        }
    }
}