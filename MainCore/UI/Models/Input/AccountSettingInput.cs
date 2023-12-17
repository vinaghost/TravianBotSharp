using MainCore.Common.Enums;
using MainCore.UI.ViewModels.Abstract;
using MainCore.UI.ViewModels.UserControls;
using ReactiveUI;

namespace MainCore.UI.Models.Input
{
    public class AccountSettingInput : ViewModelBase
    {
        public void Set(Dictionary<AccountSettingEnums, int> settings)
        {
            Tribe.Set((TribeEnums)settings.GetValueOrDefault(AccountSettingEnums.Tribe));
            ClickDelay.Set(settings.GetValueOrDefault(AccountSettingEnums.ClickDelayMin), settings.GetValueOrDefault(AccountSettingEnums.ClickDelayMax));
            TaskDelay.Set(settings.GetValueOrDefault(AccountSettingEnums.TaskDelayMin), settings.GetValueOrDefault(AccountSettingEnums.TaskDelayMax));
            WorkTime.Set(settings.GetValueOrDefault(AccountSettingEnums.WorkTimeMin), settings.GetValueOrDefault(AccountSettingEnums.WorkTimeMax));
            SleepTime.Set(settings.GetValueOrDefault(AccountSettingEnums.SleepTimeMin), settings.GetValueOrDefault(AccountSettingEnums.SleepTimeMax));
            IsAutoLoadVillage = settings.GetValueOrDefault(AccountSettingEnums.AutoLoadVillageBuilding) == 1;
            HeadlessChrome = settings.GetValueOrDefault(AccountSettingEnums.HeadlessChrome) == 1;
            IsAutoStartAdventure = settings.GetValueOrDefault(AccountSettingEnums.AutoStartAdventure) == 1;
        }

        public Dictionary<AccountSettingEnums, int> Get()
        {
            var tribe = (int)Tribe.Get();
            var (clickDelayMin, clickDelayMax) = ClickDelay.Get();
            var (taskDelayMin, taskDelayMax) = TaskDelay.Get();
            var isAutoLoadVillage = IsAutoLoadVillage ? 1 : 0;
            var (workTimeMin, workTimeMax) = WorkTime.Get();
            var (sleepTimeMin, sleepTimeMax) = SleepTime.Get();
            var headlessChrome = HeadlessChrome ? 1 : 0;
            var autoStartAdventure = IsAutoStartAdventure ? 1 : 0;
            var settings = new Dictionary<AccountSettingEnums, int>()
            {
                { AccountSettingEnums.ClickDelayMin, clickDelayMin },
                { AccountSettingEnums.ClickDelayMax, clickDelayMax },
                { AccountSettingEnums.TaskDelayMin, taskDelayMin },
                { AccountSettingEnums.TaskDelayMax, taskDelayMax },
                { AccountSettingEnums.WorkTimeMax, workTimeMax },
                { AccountSettingEnums.WorkTimeMin, workTimeMin },
                { AccountSettingEnums.SleepTimeMax, sleepTimeMax },
                { AccountSettingEnums.SleepTimeMin, sleepTimeMin },
                { AccountSettingEnums.AutoLoadVillageBuilding, isAutoLoadVillage },
                { AccountSettingEnums.Tribe, tribe },
                { AccountSettingEnums.HeadlessChrome, headlessChrome },
                { AccountSettingEnums.AutoStartAdventure, autoStartAdventure },
            };
            return settings;
        }

        public TribeSelectorViewModel Tribe { get; } = new();
        public RangeInputViewModel ClickDelay { get; } = new();
        public RangeInputViewModel TaskDelay { get; } = new();

        public RangeInputViewModel WorkTime { get; } = new();
        public RangeInputViewModel SleepTime { get; } = new();

        private bool _isAutoLoadVillage;

        public bool IsAutoLoadVillage
        {
            get => _isAutoLoadVillage;
            set => this.RaiseAndSetIfChanged(ref _isAutoLoadVillage, value);
        }

        private bool _headlessChrome;

        public bool HeadlessChrome
        {
            get => _headlessChrome;
            set => this.RaiseAndSetIfChanged(ref _headlessChrome, value);
        }

        private bool _isAutoStartAdventure;

        public bool IsAutoStartAdventure
        {
            get => _isAutoStartAdventure;
            set => this.RaiseAndSetIfChanged(ref _isAutoStartAdventure, value);
        }
    }
}