using MainCore.UI.ViewModels.Abstract;
using MainCore.UI.ViewModels.UserControls;

namespace MainCore.UI.Models.Input
{
    public partial class AccountSettingInput : ViewModelBase
    {
        public void Set(Dictionary<AccountSettingEnums, int> settings)
        {
            Tribe.Set((TribeEnums)settings.GetValueOrDefault(AccountSettingEnums.Tribe));
            ClickDelay.Set(settings.GetValueOrDefault(AccountSettingEnums.ClickDelayMin), settings.GetValueOrDefault(AccountSettingEnums.ClickDelayMax));
            TaskDelay.Set(settings.GetValueOrDefault(AccountSettingEnums.TaskDelayMin), settings.GetValueOrDefault(AccountSettingEnums.TaskDelayMax));
            WorkTime.Set(settings.GetValueOrDefault(AccountSettingEnums.WorkTimeMin), settings.GetValueOrDefault(AccountSettingEnums.WorkTimeMax));
            SleepTime.Set(settings.GetValueOrDefault(AccountSettingEnums.SleepTimeMin), settings.GetValueOrDefault(AccountSettingEnums.SleepTimeMax));
            EnableAutoLoadVillage = settings.GetValueOrDefault(AccountSettingEnums.EnableAutoLoadVillageBuilding) == 1;
            HeadlessChrome = settings.GetValueOrDefault(AccountSettingEnums.HeadlessChrome) == 1;
            EnableAutoStartAdventure = settings.GetValueOrDefault(AccountSettingEnums.EnableAutoStartAdventure) == 1;
            EnableAutoClaimDailyQuest = settings.GetValueOrDefault(AccountSettingEnums.EnableAutoClaimDailyQuest) == 1;
            EnableAutoSetHeroPoint = settings.GetValueOrDefault(AccountSettingEnums.EnableAutoSetHeroPoint) == 1;

            FarmInterval.Set(settings.GetValueOrDefault(AccountSettingEnums.FarmIntervalMin), settings.GetValueOrDefault(AccountSettingEnums.FarmIntervalMax));
            UseStartAllButton = settings.GetValueOrDefault(AccountSettingEnums.UseStartAllButton) == 1;

            InactiveFinderRadius.Set(settings.GetValueOrDefault(AccountSettingEnums.InactiveFinderRadius));
            InactiveFinderPopulation.Set(settings.GetValueOrDefault(AccountSettingEnums.InactiveFinderMinPopulation), settings.GetValueOrDefault(AccountSettingEnums.InactiveFinderMaxPopulation));
            InactiveFinderPlayerPopulation.Set(settings.GetValueOrDefault(AccountSettingEnums.InactiveFinderMinPlayerPopulation), settings.GetValueOrDefault(AccountSettingEnums.InactiveFinderMaxPlayerPopulation));
            InactiveFinderIncludeOases = settings.GetValueOrDefault(AccountSettingEnums.InactiveFinderIncludeOases) == 1;
            InactiveFinderSkipAnimalOases = settings.GetValueOrDefault(AccountSettingEnums.InactiveFinderSkipAnimalOases) == 1;
            InactiveFinderTroopSlot.Set(settings.GetValueOrDefault(AccountSettingEnums.InactiveFinderTroopSlot));
            InactiveFinderTroopAmount.Set(settings.GetValueOrDefault(AccountSettingEnums.InactiveFinderTroopAmount));
        }

        public Dictionary<AccountSettingEnums, int> Get()
        {
            var tribe = (int)Tribe.Get();
            var (clickDelayMin, clickDelayMax) = ClickDelay.Get();
            var (taskDelayMin, taskDelayMax) = TaskDelay.Get();
            var isAutoLoadVillage = EnableAutoLoadVillage ? 1 : 0;
            var (workTimeMin, workTimeMax) = WorkTime.Get();
            var (sleepTimeMin, sleepTimeMax) = SleepTime.Get();
            var headlessChrome = HeadlessChrome ? 1 : 0;
            var autoStartAdventure = EnableAutoStartAdventure ? 1 : 0;
            var autoClaimDailyQuest = EnableAutoClaimDailyQuest ? 1 : 0;
            var enableAutoSetHeroPoint = EnableAutoSetHeroPoint ? 1 : 0;

            var (farmIntervalMin, farmIntervalMax) = FarmInterval.Get();
            var useStartAllButton = UseStartAllButton ? 1 : 0;

            var inactiveFinderRadius = InactiveFinderRadius.Get();
            var (inactiveFinderMinPopulation, inactiveFinderMaxPopulation) = InactiveFinderPopulation.Get();
            var (inactiveFinderMinPlayerPopulation, inactiveFinderMaxPlayerPopulation) = InactiveFinderPlayerPopulation.Get();
            var inactiveFinderTroopSlot = InactiveFinderTroopSlot.Get();
            var inactiveFinderTroopAmount = InactiveFinderTroopAmount.Get();

            var settings = new Dictionary<AccountSettingEnums, int>()
            {
                { AccountSettingEnums.ClickDelayMin, clickDelayMin },
                { AccountSettingEnums.ClickDelayMax, clickDelayMax },
                { AccountSettingEnums.TaskDelayMin, taskDelayMin },
                { AccountSettingEnums.TaskDelayMax, taskDelayMax },
                { AccountSettingEnums.EnableAutoLoadVillageBuilding, isAutoLoadVillage },

                { AccountSettingEnums.FarmIntervalMin, farmIntervalMin },
                { AccountSettingEnums.FarmIntervalMax, farmIntervalMax },
                { AccountSettingEnums.UseStartAllButton, useStartAllButton },

                { AccountSettingEnums.InactiveFinderRadius, inactiveFinderRadius },
                { AccountSettingEnums.InactiveFinderMinPopulation, inactiveFinderMinPopulation },
                { AccountSettingEnums.InactiveFinderMaxPopulation, inactiveFinderMaxPopulation },
                { AccountSettingEnums.InactiveFinderMinPlayerPopulation, inactiveFinderMinPlayerPopulation },
                { AccountSettingEnums.InactiveFinderMaxPlayerPopulation, inactiveFinderMaxPlayerPopulation },
                { AccountSettingEnums.InactiveFinderIncludeOases, InactiveFinderIncludeOases ? 1 : 0 },
                { AccountSettingEnums.InactiveFinderSkipAnimalOases, InactiveFinderSkipAnimalOases ? 1 : 0 },
                { AccountSettingEnums.InactiveFinderTroopSlot, inactiveFinderTroopSlot },
                { AccountSettingEnums.InactiveFinderTroopAmount, inactiveFinderTroopAmount },

                { AccountSettingEnums.Tribe, tribe },
                { AccountSettingEnums.WorkTimeMax, workTimeMax },
                { AccountSettingEnums.WorkTimeMin, workTimeMin },
                { AccountSettingEnums.SleepTimeMax, sleepTimeMax },
                { AccountSettingEnums.SleepTimeMin, sleepTimeMin },

                { AccountSettingEnums.HeadlessChrome, headlessChrome },
                { AccountSettingEnums.EnableAutoStartAdventure, autoStartAdventure },
                { AccountSettingEnums.EnableAutoClaimDailyQuest, autoClaimDailyQuest },
                { AccountSettingEnums.EnableAutoSetHeroPoint, enableAutoSetHeroPoint }
            };
            return settings;
        }

        public TribeSelectorViewModel Tribe { get; } = new();
        public RangeInputViewModel ClickDelay { get; } = new();
        public RangeInputViewModel TaskDelay { get; } = new();

        public RangeInputViewModel WorkTime { get; } = new();
        public RangeInputViewModel SleepTime { get; } = new();
        public RangeInputViewModel FarmInterval { get; } = new();
        public AmountInputViewModel InactiveFinderRadius { get; } = new();
        public RangeInputViewModel InactiveFinderPopulation { get; } = new();
        public RangeInputViewModel InactiveFinderPlayerPopulation { get; } = new();
        public AmountInputViewModel InactiveFinderTroopSlot { get; } = new();
        public AmountInputViewModel InactiveFinderTroopAmount { get; } = new();

        [Reactive]
        private bool _enableAutoLoadVillage;

        [Reactive]
        private bool _headlessChrome;

        [Reactive]
        private bool _enableAutoStartAdventure;

        [Reactive]
        private bool _useStartAllButton;

        [Reactive]
        private bool _enableAutoClaimDailyQuest;

        [Reactive]
        private bool _enableAutoSetHeroPoint;

        [Reactive]
        private bool _inactiveFinderIncludeOases;

        [Reactive]
        private bool _inactiveFinderSkipAnimalOases;
    }
}