namespace MainCore.Common.Enums
{
    public enum VillageSettingEnums
    {
        // Building
        UseHeroResourceForBuilding = 1,

        ApplyRomanQueueLogicWhenBuilding,
        UseSpecialUpgrade,

        // Complete now
        CompleteImmediately,

        // General
        Tribe,

        // Train troop
        TrainTroopEnable,

        TrainTroopRepeatTimeMin,
        TrainTroopRepeatTimeMax,
        TrainWhenLowResource,

        BarrackTroop,
        BarrackAmountMin,
        BarrackAmountMax,

        StableTroop,
        StableAmountMin,
        StableAmountMax,

        WorkshopTroop,
        WorkshopAmountMin,
        WorkshopAmountMax,

        // NPC
        AutoNPCEnable,

        AutoNPCOverflow,
        AutoNPCGranaryPercent,
        AutoNPCWood,
        AutoNPCClay,
        AutoNPCIron,
        AutoNPCCrop,

        // Refresh
        AutoRefreshEnable,

        AutoRefreshMin,
        AutoRefreshMax,

        // Claim quest
        AutoClaimQuestEnable,

        CompleteImmediatelyTime,

        TrainTroopBatch,
        TrainTroopBatchSize,

        TrainTroopWaitBuilding,
        ResearchTroopWaitBuilding,
        CelebrationWaitBuilding,

        AutoTrainSettle,
        AutoSendSettle,

        EnableDonateResource,
    }
}