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
            EnableAutoLoadVillage = settings.GetValueOrDefault(AccountSettingEnums.EnableAutoLoadVillageBuilding) == 1;
            HeadlessChrome = settings.GetValueOrDefault(AccountSettingEnums.HeadlessChrome) == 1;

            EnableAutoStartAdventure = settings.GetValueOrDefault(AccountSettingEnums.EnableAutoStartAdventure) == 1;

            EnableAutoSetHeroPoint = settings.GetValueOrDefault(AccountSettingEnums.EnableAutoSetHeroPoint) == 1;
            HeroFightingPoint = settings.GetValueOrDefault(AccountSettingEnums.HeroFightingPoint);
            HeroOffPoint = settings.GetValueOrDefault(AccountSettingEnums.HeroOffPoint);
            HeroDefPoint = settings.GetValueOrDefault(AccountSettingEnums.HeroDefPoint);
            HeroResourcePoint = settings.GetValueOrDefault(AccountSettingEnums.HeroResourcePoint);

            EnableAutoReviveHero = settings.GetValueOrDefault(AccountSettingEnums.EnableAutoReviveHero) == 1;
            UseHeroResourceToRevive = settings.GetValueOrDefault(AccountSettingEnums.UseHeroResourceToRevive) == 1;
            HeroRespawnVillage.Set(settings.GetValueOrDefault(AccountSettingEnums.HeroRespawnVillage));
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
            var enableAutoSetHeroPoint = EnableAutoSetHeroPoint ? 1 : 0;
            var heroFightingPoint = HeroFightingPoint;
            var heroOffPoint = HeroOffPoint;
            var heroDefPoint = HeroDefPoint;
            var heroResourcePoint = HeroResourcePoint;

            var enableAutoReviveHero = EnableAutoReviveHero ? 1 : 0;
            var useHeroResourceToRevive = UseHeroResourceToRevive ? 1 : 0;
            var heroRespawnVillage = HeroRespawnVillage.Get();
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
                { AccountSettingEnums.EnableAutoLoadVillageBuilding, isAutoLoadVillage },
                { AccountSettingEnums.Tribe, tribe },
                { AccountSettingEnums.HeadlessChrome, headlessChrome },
                { AccountSettingEnums.EnableAutoStartAdventure, autoStartAdventure },
                { AccountSettingEnums.EnableAutoSetHeroPoint, enableAutoSetHeroPoint },
                { AccountSettingEnums.HeroFightingPoint, heroFightingPoint },
                { AccountSettingEnums.HeroOffPoint, heroOffPoint },
                { AccountSettingEnums.HeroDefPoint, heroDefPoint },
                { AccountSettingEnums.HeroResourcePoint, heroResourcePoint },
                { AccountSettingEnums.EnableAutoReviveHero, enableAutoReviveHero },
                { AccountSettingEnums.UseHeroResourceToRevive, useHeroResourceToRevive },
                { AccountSettingEnums.HeroRespawnVillage, heroRespawnVillage },
            };
            return settings;
        }

        public TribeSelectorViewModel Tribe { get; } = new();
        public RangeInputViewModel ClickDelay { get; } = new();
        public RangeInputViewModel TaskDelay { get; } = new();

        public RangeInputViewModel WorkTime { get; } = new();
        public RangeInputViewModel SleepTime { get; } = new();

        private bool _enableAutoLoadVillage;

        public bool EnableAutoLoadVillage
        {
            get => _enableAutoLoadVillage;
            set => this.RaiseAndSetIfChanged(ref _enableAutoLoadVillage, value);
        }

        private bool _headlessChrome;

        public bool HeadlessChrome
        {
            get => _headlessChrome;
            set => this.RaiseAndSetIfChanged(ref _headlessChrome, value);
        }

        private bool _enableAutoStartAdventure;

        public bool EnableAutoStartAdventure
        {
            get => _enableAutoStartAdventure;
            set => this.RaiseAndSetIfChanged(ref _enableAutoStartAdventure, value);
        }

        private bool _enableAutoSetHeroPoint;

        public bool EnableAutoSetHeroPoint
        {
            get => _enableAutoSetHeroPoint;
            set => this.RaiseAndSetIfChanged(ref _enableAutoSetHeroPoint, value);
        }

        private int _heroFightingPoint;

        public int HeroFightingPoint
        {
            get => _heroFightingPoint;
            set => this.RaiseAndSetIfChanged(ref _heroFightingPoint, value);
        }

        private int _heroOffPoint;

        public int HeroOffPoint
        {
            get => _heroOffPoint;
            set => this.RaiseAndSetIfChanged(ref _heroOffPoint, value);
        }

        private int _heroDefPoint;

        public int HeroDefPoint
        {
            get => _heroDefPoint;
            set => this.RaiseAndSetIfChanged(ref _heroDefPoint, value);
        }

        private int _heroResourcePoint;

        public int HeroResourcePoint
        {
            get => _heroResourcePoint;
            set => this.RaiseAndSetIfChanged(ref _heroResourcePoint, value);
        }

        private bool _enableAutoReviveHero;

        public bool EnableAutoReviveHero
        {
            get => _enableAutoReviveHero;
            set => this.RaiseAndSetIfChanged(ref _enableAutoReviveHero, value);
        }

        private bool _useHeroResourceToRevive;

        public bool UseHeroResourceToRevive
        {
            get => _useHeroResourceToRevive;
            set => this.RaiseAndSetIfChanged(ref _useHeroResourceToRevive, value);
        }

        public VillageSelectorViewModel HeroRespawnVillage { get; } = new();
    }
}