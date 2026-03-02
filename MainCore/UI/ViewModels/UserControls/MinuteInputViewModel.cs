using MainCore.UI.ViewModels.Abstract;

namespace MainCore.UI.ViewModels.UserControls
{
    public partial class MinuteInputViewModel : ViewModelBase
    {
        public MinuteInputViewModel()
        {
            IncreaseMinuteCommand = ReactiveCommand.Create(IncreaseMinute);
            DecreaseMinuteCommand = ReactiveCommand.Create(DecreaseMinute);
        }

        public void Set(int minute)
        {
            Minute = Math.Clamp(minute, 0, 59);
        }

        public int Get()
        {
            return Minute;
        }

        private void IncreaseMinute()
        {
            Minute = Minute >= 59 ? 0 : Minute + 1;
        }

        private void DecreaseMinute()
        {
            Minute = Minute <= 0 ? 59 : Minute - 1;
        }

        public ReactiveCommand<Unit, Unit> IncreaseMinuteCommand { get; }
        public ReactiveCommand<Unit, Unit> DecreaseMinuteCommand { get; }

        [Reactive]
        private int _minute = 0;
    }
}