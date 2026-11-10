using System;
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

    public static bool IsNPCEnable(this AppDbContext context, VillageId villageId)
    {
        return context.BooleanByName(villageId, VillageSettingEnums.AutoNPCEnable);
    }

    public static bool IsRefreshENable(this AppDbContext context, VillageId villageId)
    {
        return context.BooleanByName(villageId, VillageSettingEnums.AutoRefreshEnable);
    }
}