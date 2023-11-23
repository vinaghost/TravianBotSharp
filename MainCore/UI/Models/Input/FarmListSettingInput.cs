using MainCore.Common.Enums;
using MainCore.UI.ViewModels.Abstract;
using MainCore.UI.ViewModels.UserControls;
using ReactiveUI;
using System.Collections.Generic;

namespace MainCore.UI.Models.Input
{
    public class FarmListSettingInput : ViewModelBase
    {
        public RangeInputViewModel FarmInterval { get; } = new();
        private bool _useStartAllButton;

        public bool UseStartAllButton
        {
            get => _useStartAllButton;
            set => this.RaiseAndSetIfChanged(ref _useStartAllButton, value);
        }

        public void Set(Dictionary<AccountSettingEnums, int> settings)
        {
            FarmInterval.Set(settings.GetValueOrDefault(AccountSettingEnums.FarmIntervalMin), settings.GetValueOrDefault(AccountSettingEnums.FarmIntervalMax));
            UseStartAllButton = settings.GetValueOrDefault(AccountSettingEnums.UseStartAllButton) == 1;
        }

        public Dictionary<AccountSettingEnums, int> Get()
        {
            var (farmIntervalMin, farmIntervalMax) = FarmInterval.Get();
            var useStartAllButton = UseStartAllButton ? 1 : 0;

            var settings = new Dictionary<AccountSettingEnums, int>()
            {
                { AccountSettingEnums.FarmIntervalMin, farmIntervalMin },
                { AccountSettingEnums.FarmIntervalMax, farmIntervalMax },
                { AccountSettingEnums.UseStartAllButton, useStartAllButton },
            };
            return settings;
        }
    }
}