using System;
using System.Collections.Frozen;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace MainCore.Features.Shared.Tasks;

public static class TaskSettingExtension
{
    public static bool IsStartAdventureEnable(this AppDbContext context, AccountId accountId)
    {
        return context.BooleanByName(accountId, AccountSettingEnums.EnableAutoStartAdventure);
    }

    public static bool IsDailyQuestEnable(this AppDbContext context, AccountId accountId)
    {
        return context.BooleanByName(accountId, AccountSettingEnums.EnableAutoClaimDailyQuest);
    }

    public static bool IsSetHeroPointEnable(this AppDbContext context, AccountId accountId)
    {
        return context.BooleanByName(accountId, AccountSettingEnums.EnableAutoSetHeroPoint);
    }

    public static bool IsVillageQuestEnable(this AppDbContext context, VillageId villageId)
    {
        return context.BooleanByName(villageId, VillageSettingEnums.AutoClaimQuestEnable);
    }

    public static bool IsNPCEnable(this AppDbContext context, AccountId accountId, VillageId villageId)
    {
        var settingEnable = context.BooleanByName(villageId, VillageSettingEnums.AutoNPCEnable);
        if (!settingEnable) return false;

        var gold = context.AccountsInfo
            .Where(x => x.AccountId == accountId.Value)
            .Select(x => x.Gold)
            .FirstOrDefault();
        if (gold < 3) return false;

        var granaryPercent = (int)context.Storages
           .Where(x => x.VillageId == villageId.Value)
           .Select(x => x.Crop * 100f / x.Granary)
           .FirstOrDefault();

        var autoNPCGranaryPercent = context.ByName(villageId, VillageSettingEnums.AutoNPCGranaryPercent);
        if (granaryPercent < autoNPCGranaryPercent) return false;

        return true;
    }

    public static bool IsRefreshEnable(this AppDbContext context, VillageId villageId)
    {
        return context.BooleanByName(villageId, VillageSettingEnums.AutoRefreshEnable);
    }

    public static bool IsTrainTroopEnable(this AppDbContext context, VillageId villageId)
    {
        return context.BooleanByName(villageId, VillageSettingEnums.TrainTroopEnable);
    }

    private static readonly FrozenSet<BuildingEnums> UnskippableBuildings =
    [
        BuildingEnums.Residence,
        BuildingEnums.Palace,
        BuildingEnums.CommandCenter,
    ];

    public static bool IsInstantUpgradeEnable(this AppDbContext context, AccountId accountId, VillageId villageId)
    {
        var settingEnable = context.BooleanByName(villageId, VillageSettingEnums.CompleteImmediately);
        if (!settingEnable) return false;

        var gold = context.AccountsInfo
            .Where(x => x.AccountId == accountId.Value)
            .Select(x => x.Gold)
            .FirstOrDefault();
        if (gold < 2) return false;

        var queueBuildings = context.QueueBuildings
            .Where(x => x.VillageId == villageId.Value)
            .ToList();

        if (queueBuildings.Count == 0) return false;
        if (queueBuildings.Any(x => UnskippableBuildings.Contains(x.Type))) return false;

        var completeImmediatelyMinimumTime = context.ByName(villageId, VillageSettingEnums.CompleteImmediatelyTime);
        var requiredTime = DateTime.Now.AddMinutes(completeImmediatelyMinimumTime);

        var anyBuilding = queueBuildings
            .Any(x => x.CompleteTime > requiredTime);

        return anyBuilding;
    }
}