namespace MainCore.Common.Enums
{
    public enum VillageSettingEnums
    {
        // Building
        UseHeroResourceForBuilding = 1,

        ApplyRomanQueueLogicWhenBuilding,
        UseSpecialUpgrade,

        // Complete now
        InstantUpgrade,

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
    }
}