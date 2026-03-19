using MainCore.UI.ViewModels.Abstract;

namespace MainCore.UI.ViewModels.UserControls
{
    public partial class HourInputViewModel : ViewModelBase
    {
        public HourInputViewModel()
        {
            IncreaseHourCommand = ReactiveCommand.Create(IncreaseHour);
            DecreaseHourCommand = ReactiveCommand.Create(DecreaseHour);
        }

        public void Set(int hour)
        {
            Hour = Math.Clamp(hour, 0, 23);
        }

        public int Get()
        {
            return Hour;
        }

        private void IncreaseHour()
        {
            Hour = Hour >= 23 ? 0 : Hour + 1;
        }

        private void DecreaseHour()
        {
            Hour = Hour <= 0 ? 23 : Hour - 1;
        }

        public ReactiveCommand<Unit, Unit> IncreaseHourCommand { get; }
        public ReactiveCommand<Unit, Unit> DecreaseHourCommand { get; }

        [Reactive]
        private int _hour = 6;
    }
}
