using MainCore.Common.Enums;
using MainCore.UI.ViewModels.Abstract;
using MainCore.UI.ViewModels.UserControls;
using ReactiveUI;

namespace MainCore.UI.Models.Input
{
    public class TrainTroopInput : ViewModelBase
    {
        public TroopSelectorBasedOnTribeViewModel Type { get; } = new();

        private bool _great;

        public bool Great
        {
            get => _great;
            set => this.RaiseAndSetIfChanged(ref _great, value);
        }

        private int _amount;

        public int Amount
        {
            get => _amount;
            set => this.RaiseAndSetIfChanged(ref _amount, value);
        }

        public void Set(TribeEnums tribe)
        {
            Type.Set(tribe);
        }

        public (TroopEnums, int, bool) Get()
        {
            return (Type.Get(), Amount, Great);
        }
    }
}