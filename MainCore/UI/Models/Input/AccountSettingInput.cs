using MainCore.UI.ViewModels.Abstract;
using MainCore.UI.ViewModels.UserControls;

namespace MainCore.UI.Models.Input
{
    public partial class AccountSettingInput : ViewModelBase
    {
        public AccountSettingInput()
        {
            WorkStartHour.Set(6);
            WorkStartMinute.Set(0);
            WorkEndHour.Set(22);
            WorkEndMinute.Set(0);
            RandomMinute.Set(60);
        }

        public void Set(Dictionary<AccountSettingEnums, int> settings)
        {
            Tribe.Set((TribeEnums)settings.GetValueOrDefault(AccountSettingEnums.Tribe));
            ClickDelay.Set(settings.GetValueOrDefault(AccountSettingEnums.ClickDelayMin), settings.GetValueOrDefault(AccountSettingEnums.ClickDelayMax));
            TaskDelay.Set(settings.GetValueOrDefault(AccountSettingEnums.TaskDelayMin), settings.GetValueOrDefault(AccountSettingEnums.TaskDelayMax));
            WorkTime.Set(settings.GetValueOrDefault(AccountSettingEnums.WorkTimeMin), settings.GetValueOrDefault(AccountSettingEnums.WorkTimeMax));
            SleepTime.Set(settings.GetValueOrDefault(AccountSettingEnums.SleepTimeMin), settings.GetValueOrDefault(AccountSettingEnums.SleepTimeMax));
            WorkStartHour.Set(settings.GetValueOrDefault(AccountSettingEnums.WorkStartHour, 6));
            WorkStartMinute.Set(settings.GetValueOrDefault(AccountSettingEnums.WorkStartMinute, 0));
            WorkEndHour.Set(settings.GetValueOrDefault(AccountSettingEnums.WorkEndHour, 22));
            WorkEndMinute.Set(settings.GetValueOrDefault(AccountSettingEnums.WorkEndMinute, 0));
            RandomMinute.Set(settings.GetValueOrDefault(AccountSettingEnums.SleepRandomMinute, 60));
            EnableAutoLoadVillage = settings.GetValueOrDefault(AccountSettingEnums.EnableAutoLoadVillageBuilding) == 1;
            HeadlessChrome = settings.GetValueOrDefault(AccountSettingEnums.HeadlessChrome) == 1;
            EnableAutoStartAdventure = settings.GetValueOrDefault(AccountSettingEnums.EnableAutoStartAdventure) == 1;
            FarmInterval.Set(settings.GetValueOrDefault(AccountSettingEnums.FarmIntervalMin), settings.GetValueOrDefault(AccountSettingEnums.FarmIntervalMax));
            UseStartAllButton = settings.GetValueOrDefault(AccountSettingEnums.UseStartAllButton) == 1;
        }

        public Dictionary<AccountSettingEnums, int> Get()
        {
            var tribe = (int)Tribe.Get();
            var (clickDelayMin, clickDelayMax) = ClickDelay.Get();
            var (taskDelayMin, taskDelayMax) = TaskDelay.Get();
            var isAutoLoadVillage = EnableAutoLoadVillage ? 1 : 0;
            var (workTimeMin, workTimeMax) = WorkTime.Get();
            var (sleepTimeMin, sleepTimeMax) = SleepTime.Get();
            var workStartHour = WorkStartHour.Get();
            var workEndHour = WorkEndHour.Get();
            var headlessChrome = HeadlessChrome ? 1 : 0;
            var autoStartAdventure = EnableAutoStartAdventure ? 1 : 0;

            var (farmIntervalMin, farmIntervalMax) = FarmInterval.Get();
            var useStartAllButton = UseStartAllButton ? 1 : 0;

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

                { AccountSettingEnums.Tribe, tribe },
                { AccountSettingEnums.WorkTimeMax, workTimeMax },
                { AccountSettingEnums.WorkTimeMin, workTimeMin },
                { AccountSettingEnums.SleepTimeMax, sleepTimeMax },
                { AccountSettingEnums.SleepTimeMin, sleepTimeMin },
                { AccountSettingEnums.WorkStartHour, workStartHour },
                { AccountSettingEnums.WorkStartMinute, WorkStartMinute.Get() },
                { AccountSettingEnums.WorkEndHour, workEndHour },
                { AccountSettingEnums.WorkEndMinute, WorkEndMinute.Get() },
                { AccountSettingEnums.SleepRandomMinute, RandomMinute.Get() },

                { AccountSettingEnums.HeadlessChrome, headlessChrome },
                { AccountSettingEnums.EnableAutoStartAdventure, autoStartAdventure },
            };
            return settings;
        }

        public TribeSelectorViewModel Tribe { get; } = new();
        public RangeInputViewModel ClickDelay { get; } = new();
        public RangeInputViewModel TaskDelay { get; } = new();

        public RangeInputViewModel WorkTime { get; } = new();
        public RangeInputViewModel SleepTime { get; } = new();
        public HourInputViewModel WorkStartHour { get; } = new();
        public MinuteInputViewModel WorkStartMinute { get; } = new();
        public HourInputViewModel WorkEndHour { get; } = new();
        public MinuteInputViewModel WorkEndMinute { get; } = new();
        public MinuteInputViewModel RandomMinute { get; } = new();
        public RangeInputViewModel FarmInterval { get; } = new();

        [Reactive]
        private bool _enableAutoLoadVillage;

        [Reactive]
        private bool _headlessChrome;

        [Reactive]
        private bool _enableAutoStartAdventure;

        [Reactive]
        private bool _useStartAllButton;
    }
}