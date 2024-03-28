using MainCore.Common.Enums;
using MainCore.UI.ViewModels.Abstract;
using MainCore.UI.ViewModels.UserControls;

namespace MainCore.UI.Models.Input
{
    public class ResearchTroopInput : ViewModelBase
    {
        public TroopSelectorBasedOnTribeViewModel Type { get; } = new();

        public void Set(TribeEnums tribe)
        {
            Type.Set(tribe);
        }

        public TroopEnums Get()
        {
            return Type.Get();
        }
    }
}