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

            EquipGearBeforeStartAdventure = settings.GetValueOrDefault(AccountSettingEnums.EquipGearBeforeStartAdventure) == 1;

            HealingBeforeStartAdventure = settings.GetValueOrDefault(AccountSettingEnums.HealingBeforeStartAdventure) == 1;
            HealthBeforeStartAdventure = settings.GetValueOrDefault(AccountSettingEnums.HealthBeforeStartAdventure);
            EnableDiscordAlert = settings.GetValueOrDefault(AccountSettingEnums.EnableDiscordAlert) == 1;

            EnableStopAlert = settings.GetValueOrDefault(AccountSettingEnums.EnableStopAlert) == 1;
            Bonus.Set((AllianceBonusEnums)settings.GetValueOrDefault(AccountSettingEnums.DonateResourceType));
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

            var equipGearBeforeStartAdventure = EquipGearBeforeStartAdventure ? 1 : 0;

            var healingBeforeStartAdventure = HealingBeforeStartAdventure ? 1 : 0;
            var healthBeforeStartAdventure = HealthBeforeStartAdventure;
            var enableDiscordAlert = EnableDiscordAlert ? 1 : 0;
            var enableStopAlert = EnableStopAlert ? 1 : 0;
            var donateResourceType = (int)Bonus.Get();
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
                { AccountSettingEnums.EquipGearBeforeStartAdventure, equipGearBeforeStartAdventure },
                { AccountSettingEnums.HealingBeforeStartAdventure, healingBeforeStartAdventure },
                { AccountSettingEnums.HealthBeforeStartAdventure,  healthBeforeStartAdventure},
                { AccountSettingEnums.EnableDiscordAlert, enableDiscordAlert},
                { AccountSettingEnums.EnableStopAlert, enableStopAlert},
                { AccountSettingEnums.DonateResourceType, donateResourceType},
            };
            return settings;
        }

        public TribeSelectorViewModel Tribe { get; } = new();
        public BonusSelectorViewModel Bonus { get; } = new();
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

        private bool _equipGearBeforeStartAdventure;

        public bool EquipGearBeforeStartAdventure
        {
            get => _equipGearBeforeStartAdventure;
            set => this.RaiseAndSetIfChanged(ref _equipGearBeforeStartAdventure, value);
        }

        private int _healthBeforeStartAdventure;

        public int HealthBeforeStartAdventure
        {
            get => _healthBeforeStartAdventure;
            set => this.RaiseAndSetIfChanged(ref _healthBeforeStartAdventure, value);
        }

        private bool _healingBeforeStartAdventure;

        public bool HealingBeforeStartAdventure
        {
            get => _healingBeforeStartAdventure;
            set => this.RaiseAndSetIfChanged(ref _healingBeforeStartAdventure, value);
        }

        private bool _enableDiscordAlert;

        public bool EnableDiscordAlert
        {
            get => _enableDiscordAlert;
            set => this.RaiseAndSetIfChanged(ref _enableDiscordAlert, value);
        }

        private bool _enableDonateResource;

        public bool EnableStopAlert
        {
            get => _enableDonateResource;
            set => this.RaiseAndSetIfChanged(ref _enableDonateResource, value);
        }
    }
}