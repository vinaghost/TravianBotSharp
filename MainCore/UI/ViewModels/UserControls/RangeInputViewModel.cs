using MainCore.UI.ViewModels.Abstract;

namespace MainCore.UI.ViewModels.UserControls
{
    public partial class RangeInputViewModel : ViewModelBase
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

        [Reactive]
        private int _min;

        [Reactive]
        private int _max;
    }
}