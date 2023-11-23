using MainCore.UI.ViewModels.Abstract;
using ReactiveUI;

namespace MainCore.UI.ViewModels.UserControls
{
    public class RangeInputViewModel : ViewModelBase
    {
        public void Set(int min, int max)
        {
            Min = min;
            Max = max;
        }

        public (int, int) Get()
        {
            return (Min, Max);
        }

        private int _min;

        public int Min
        {
            get => _min;
            set => this.RaiseAndSetIfChanged(ref _min, value);
        }

        private int _max;

        public int Max
        {
            get => _max;
            set => this.RaiseAndSetIfChanged(ref _max, value);
        }
    }
}