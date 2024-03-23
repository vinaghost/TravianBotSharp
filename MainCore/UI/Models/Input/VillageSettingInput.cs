using MainCore.Common.Enums;
using MainCore.UI.ViewModels.Abstract;
using MainCore.UI.ViewModels.UserControls;
using ReactiveUI;
using System.Reactive.Linq;

namespace MainCore.UI.Models.Input
{
    public class VillageSettingInput : ViewModelBase
    {
        private bool _useHeroResourceForBuilding;
        private bool _applyRomanQueueLogicWhenBuilding;
        private bool _useSpecialUpgrade;
        private bool _completeImmediately;
        private int _completeImmediatelyTime;
        public TribeSelectorViewModel Tribe { get; } = new();

        private bool _trainTroopEnable;
        private bool _trainWhenLowResource;

        public TroopSelectorBasedOnBuildingViewModel BarrackTroop { get; } = new();
        public TroopSelectorBasedOnBuildingViewModel StableTroop { get; } = new();
        public TroopSelectorBasedOnBuildingViewModel WorkshopTroop { get; } = new();

        public RangeInputViewModel TrainTroopRepeatTime { get; } = new();
        public RangeInputViewModel BarrackAmount { get; } = new();
        public RangeInputViewModel StableAmount { get; } = new();
        public RangeInputViewModel WorkshopAmount { get; } = new();

        private bool _autoNPCEnable;
        private bool _autoNPCOverflow;
        public AmountInputViewModel AutoNPCGranaryPercent { get; } = new();
        public ResourceInputViewModel AutoNPCRatio { get; } = new();

        private bool _autoRefreshEnable;
        public RangeInputViewModel AutoRefreshTime { get; } = new();
        private bool _autoClaimQuestEnable;

        private bool _autoTrainSettle;

        public void Set(Dictionary<VillageSettingEnums, int> settings)
        {
            var tribe = (TribeEnums)settings.GetValueOrDefault(VillageSettingEnums.Tribe);
            Tribe.Set(tribe);

            UseHeroResourceForBuilding = settings.GetValueOrDefault(VillageSettingEnums.UseHeroResourceForBuilding) == 1;
            ApplyRomanQueueLogicWhenBuilding = settings.GetValueOrDefault(VillageSettingEnums.ApplyRomanQueueLogicWhenBuilding) == 1;
            CompleteImmediately = settings.GetValueOrDefault(VillageSettingEnums.CompleteImmediately) == 1;
            CompleteImmediatelyTime = settings.GetValueOrDefault(VillageSettingEnums.CompleteImmediatelyTime);
            UseSpecialUpgrade = settings.GetValueOrDefault(VillageSettingEnums.UseSpecialUpgrade) == 1;

            TrainTroopEnable = settings.GetValueOrDefault(VillageSettingEnums.TrainTroopEnable) == 1;
            TrainWhenLowResource = settings.GetValueOrDefault(VillageSettingEnums.TrainWhenLowResource) == 1;
            TrainTroopRepeatTime.Set(
                settings.GetValueOrDefault(VillageSettingEnums.TrainTroopRepeatTimeMin),
                settings.GetValueOrDefault(VillageSettingEnums.TrainTroopRepeatTimeMax));
            var barrackTroop = (TroopEnums)settings.GetValueOrDefault(VillageSettingEnums.BarrackTroop);
            BarrackTroop.Set(barrackTroop, BuildingEnums.Barracks, tribe);
            BarrackAmount.Set(
                settings.GetValueOrDefault(VillageSettingEnums.BarrackAmountMin),
                settings.GetValueOrDefault(VillageSettingEnums.BarrackAmountMax));
            var stableTroop = (TroopEnums)settings.GetValueOrDefault(VillageSettingEnums.StableTroop);
            StableTroop.Set(stableTroop, BuildingEnums.Stable, tribe);
            StableAmount.Set(
                settings.GetValueOrDefault(VillageSettingEnums.StableAmountMin),
                settings.GetValueOrDefault(VillageSettingEnums.StableAmountMax));
            var workshopTroop = (TroopEnums)settings.GetValueOrDefault(VillageSettingEnums.WorkshopTroop);
            WorkshopTroop.Set(workshopTroop, BuildingEnums.Workshop, tribe);
            WorkshopAmount.Set(
                settings.GetValueOrDefault(VillageSettingEnums.WorkshopAmountMin),
                settings.GetValueOrDefault(VillageSettingEnums.WorkshopAmountMax));

            AutoNPCEnable = settings.GetValueOrDefault(VillageSettingEnums.AutoNPCEnable) == 1;
            AutoNPCOverflow = settings.GetValueOrDefault(VillageSettingEnums.AutoNPCOverflow) == 1;
            AutoNPCGranaryPercent.Set(settings.GetValueOrDefault(VillageSettingEnums.AutoNPCGranaryPercent));
            AutoNPCRatio.Set(
                settings.GetValueOrDefault(VillageSettingEnums.AutoNPCWood),
                settings.GetValueOrDefault(VillageSettingEnums.AutoNPCClay),
                settings.GetValueOrDefault(VillageSettingEnums.AutoNPCIron),
                settings.GetValueOrDefault(VillageSettingEnums.AutoNPCCrop));

            AutoRefreshEnable = settings.GetValueOrDefault(VillageSettingEnums.AutoRefreshEnable) == 1;
            AutoRefreshTime.Set(
                settings.GetValueOrDefault(VillageSettingEnums.AutoRefreshMin),
                settings.GetValueOrDefault(VillageSettingEnums.AutoRefreshMax));

            AutoClaimQuestEnable = settings.GetValueOrDefault(VillageSettingEnums.AutoClaimQuestEnable) == 1;

            TrainTroopBatch = settings.GetValueOrDefault(VillageSettingEnums.TrainTroopBatch) == 1;
            TrainTroopBatchSize = settings.GetValueOrDefault(VillageSettingEnums.TrainTroopBatchSize);
            TrainTroopWaitBuilding = settings.GetValueOrDefault(VillageSettingEnums.TrainTroopWaitBuilding) == 1;
            ResearchTroopWaitBuilding = settings.GetValueOrDefault(VillageSettingEnums.ResearchTroopWaitBuilding) == 1;
            CelebrationWaitBuilding = settings.GetValueOrDefault(VillageSettingEnums.CelebrationWaitBuilding) == 1;

            AutoTrainSettle = settings.GetValueOrDefault(VillageSettingEnums.AutoTrainSettle) == 1;
        }

        public Dictionary<VillageSettingEnums, int> Get()
        {
            var useHeroResourceForBuilding = UseHeroResourceForBuilding ? 1 : 0;
            var applyRomanQueueLogicWhenBuilding = ApplyRomanQueueLogicWhenBuilding ? 1 : 0;
            var useSpecialUpgrade = UseSpecialUpgrade ? 1 : 0;
            var completeImmediately = CompleteImmediately ? 1 : 0;
            var completeImmediatelyTime = CompleteImmediatelyTime;

            var tribe = (int)Tribe.Get();

            var trainTroopEnable = TrainTroopEnable ? 1 : 0;
            var trainWhenLowResource = TrainWhenLowResource ? 1 : 0;
            var (trainTroopRepeatTimeMin, trainTroopRepeatTimeMax) = TrainTroopRepeatTime.Get();
            var barrackTroop = (int)BarrackTroop.Get();
            var (barrackAmountMin, barrackAmountMax) = BarrackAmount.Get();
            var stableTroop = (int)StableTroop.Get();
            var (stableAmountMin, stableAmountMax) = StableAmount.Get();
            var workshopTroop = (int)WorkshopTroop.Get();
            var (workshopAmountMin, workshopAmountMax) = WorkshopAmount.Get();

            var autoNPCEnable = AutoNPCEnable ? 1 : 0;
            var autoNPCOverflow = AutoNPCOverflow ? 1 : 0;
            var autoNPCGranaryPercent = AutoNPCGranaryPercent.Get();
            var (autoNPCWood, autoNPCClay, autoNPCIron, autoNPCCrop) = AutoNPCRatio.Get();

            var autoRefreshEnable = AutoRefreshEnable ? 1 : 0;
            var (autoRefreshMin, autoRefreshMax) = AutoRefreshTime.Get();

            var autoClaimQuestEnable = AutoClaimQuestEnable ? 1 : 0;

            var trainTroopBatch = TrainTroopBatch ? 1 : 0;
            var trainTroopBatchSize = TrainTroopBatchSize;
            var trainTroopWaitBuilding = TrainTroopWaitBuilding ? 1 : 0;
            var researchTroopWaitBuilding = ResearchTroopWaitBuilding ? 1 : 0;
            var celebrationWaitBuilding = CelebrationWaitBuilding ? 1 : 0;

            var autoTrainSettle = AutoTrainSettle ? 1 : 0;
            var settings = new Dictionary<VillageSettingEnums, int>()
            {
                { VillageSettingEnums.UseHeroResourceForBuilding, useHeroResourceForBuilding },
                { VillageSettingEnums.ApplyRomanQueueLogicWhenBuilding, applyRomanQueueLogicWhenBuilding },
                { VillageSettingEnums.UseSpecialUpgrade, useSpecialUpgrade },
                { VillageSettingEnums.CompleteImmediately, completeImmediately },
                { VillageSettingEnums.CompleteImmediatelyTime, completeImmediatelyTime },
                { VillageSettingEnums.Tribe, tribe },
                { VillageSettingEnums.TrainTroopEnable, trainTroopEnable },
                { VillageSettingEnums.TrainWhenLowResource, trainWhenLowResource },
                { VillageSettingEnums.TrainTroopRepeatTimeMin, trainTroopRepeatTimeMin },
                { VillageSettingEnums.TrainTroopRepeatTimeMax, trainTroopRepeatTimeMax },
                { VillageSettingEnums.BarrackTroop, barrackTroop },
                { VillageSettingEnums.BarrackAmountMin, barrackAmountMin },
                { VillageSettingEnums.BarrackAmountMax, barrackAmountMax },
                { VillageSettingEnums.StableTroop, stableTroop },
                { VillageSettingEnums.StableAmountMin, stableAmountMin },
                { VillageSettingEnums.StableAmountMax, stableAmountMax },
                { VillageSettingEnums.WorkshopTroop, workshopTroop },
                { VillageSettingEnums.WorkshopAmountMin, workshopAmountMin },
                { VillageSettingEnums.WorkshopAmountMax, workshopAmountMax },
                { VillageSettingEnums.AutoNPCEnable, autoNPCEnable },
                { VillageSettingEnums.AutoNPCOverflow, autoNPCOverflow },
                { VillageSettingEnums.AutoNPCGranaryPercent, autoNPCGranaryPercent },
                { VillageSettingEnums.AutoNPCWood, autoNPCWood },
                { VillageSettingEnums.AutoNPCClay, autoNPCClay },
                { VillageSettingEnums.AutoNPCIron, autoNPCIron },
                { VillageSettingEnums.AutoNPCCrop, autoNPCCrop },
                { VillageSettingEnums.AutoRefreshEnable, autoRefreshEnable },
                { VillageSettingEnums.AutoRefreshMin, autoRefreshMin },
                { VillageSettingEnums.AutoRefreshMax, autoRefreshMax },
                { VillageSettingEnums.AutoClaimQuestEnable, autoClaimQuestEnable },
                { VillageSettingEnums.TrainTroopBatch, trainTroopBatch },
                { VillageSettingEnums.TrainTroopBatchSize, trainTroopBatchSize },
                { VillageSettingEnums.TrainTroopWaitBuilding, trainTroopWaitBuilding },
                { VillageSettingEnums.ResearchTroopWaitBuilding, researchTroopWaitBuilding },
                { VillageSettingEnums.CelebrationWaitBuilding, celebrationWaitBuilding },
                { VillageSettingEnums.AutoTrainSettle, autoTrainSettle },
            };
            return settings;
        }

        public VillageSettingInput()
        {
            this.WhenAnyValue(vm => vm.Tribe.SelectedItem)
                .Select(x => x.Tribe)
                .Subscribe((tribe) =>
                {
                    BarrackTroop.ChangeTribe(BuildingEnums.Barracks, tribe);
                    StableTroop.ChangeTribe(BuildingEnums.Stable, tribe);
                    WorkshopTroop.ChangeTribe(BuildingEnums.Workshop, tribe);
                });
        }

        public bool UseHeroResourceForBuilding
        {
            get => _useHeroResourceForBuilding;
            set => this.RaiseAndSetIfChanged(ref _useHeroResourceForBuilding, value);
        }

        public bool CompleteImmediately
        {
            get => _completeImmediately;
            set => this.RaiseAndSetIfChanged(ref _completeImmediately, value);
        }

        public int CompleteImmediatelyTime
        {
            get => _completeImmediatelyTime;
            set => this.RaiseAndSetIfChanged(ref _completeImmediatelyTime, value);
        }

        public bool ApplyRomanQueueLogicWhenBuilding
        {
            get => _applyRomanQueueLogicWhenBuilding;
            set => this.RaiseAndSetIfChanged(ref _applyRomanQueueLogicWhenBuilding, value);
        }

        public bool UseSpecialUpgrade
        {
            get => _useSpecialUpgrade;
            set => this.RaiseAndSetIfChanged(ref _useSpecialUpgrade, value);
        }

        public bool TrainTroopEnable
        {
            get => _trainTroopEnable;
            set => this.RaiseAndSetIfChanged(ref _trainTroopEnable, value);
        }

        public bool TrainWhenLowResource
        {
            get => _trainWhenLowResource;
            set => this.RaiseAndSetIfChanged(ref _trainWhenLowResource, value);
        }

        public bool AutoNPCEnable
        {
            get => _autoNPCEnable;
            set => this.RaiseAndSetIfChanged(ref _autoNPCEnable, value);
        }

        public bool AutoNPCOverflow
        {
            get => _autoNPCOverflow;
            set => this.RaiseAndSetIfChanged(ref _autoNPCOverflow, value);
        }

        public bool AutoRefreshEnable
        {
            get => _autoRefreshEnable;
            set => this.RaiseAndSetIfChanged(ref _autoRefreshEnable, value);
        }

        public bool AutoClaimQuestEnable
        {
            get => _autoClaimQuestEnable;
            set => this.RaiseAndSetIfChanged(ref _autoClaimQuestEnable, value);
        }

        private bool _trainTroopBatch;

        public bool TrainTroopBatch
        {
            get => _trainTroopBatch;
            set => this.RaiseAndSetIfChanged(ref _trainTroopBatch, value);
        }

        private int _trainTroopBatchSize;

        public int TrainTroopBatchSize
        {
            get => _trainTroopBatchSize;
            set => this.RaiseAndSetIfChanged(ref _trainTroopBatchSize, value);
        }

        private bool _trainTroopWaitBuilding;

        public bool TrainTroopWaitBuilding
        {
            get => _trainTroopWaitBuilding;
            set => this.RaiseAndSetIfChanged(ref _trainTroopWaitBuilding, value);
        }

        private bool _researchTroopWaitBuilding;

        public bool ResearchTroopWaitBuilding
        {
            get => _researchTroopWaitBuilding;
            set => this.RaiseAndSetIfChanged(ref _researchTroopWaitBuilding, value);
        }

        private bool _celebrationWaitBuilding;

        public bool CelebrationWaitBuilding
        {
            get => _celebrationWaitBuilding;
            set => this.RaiseAndSetIfChanged(ref _celebrationWaitBuilding, value);
        }

        public bool AutoTrainSettle
        {
            get => _autoTrainSettle;
            set => this.RaiseAndSetIfChanged(ref _autoTrainSettle, value);
        }
    }
}