using MainCore.UI.ViewModels.Abstract;

namespace MainCore.UI.ViewModels.UserControls
{
    public partial class AmountInputViewModel : ViewModelBase
    {
        [Reactive]
        private int _value;

        public int Get() => Value;

        public void Set(int value) => Value = value;
    }
}