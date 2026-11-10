using MainCore.Features.ClaimDailyQuest;
using MainCore.Features.SetHeroPoint;
using MainCore.Features.StartAdventure;
using MainCore.Features.Update;

namespace MainCore.UI.ViewModels.Tabs
{
    public static class AccountSettingViewModelExtension
    {
        extension(AppDbContext context)
        {
            public void TriggerTask(TaskManager taskManager, AccountId accountId, Dictionary<AccountSettingEnums, int> settings)
            {
                if (settings.Count == 0) return;

                if (settings.ContainsKey(AccountSettingEnums.EnableAutoLoadVillageBuilding))
                {
                    if (settings[AccountSettingEnums.EnableAutoLoadVillageBuilding] == 1)
                    {
                        var villages = context.Villages
                            .Where(x => x.AccountId == accountId.Value)
                            .Where(x => x.Buildings.Count < 40)
                            .Select(x => new VillageId(x.Id));

                        foreach (var village in villages)
                        {
                            if (!taskManager.IsExist<UpdateBuildingTask.Task>(accountId, village))
                            {
                                taskManager.Add(new UpdateBuildingTask.Task(accountId, village));
                            }
                        }
                    }
                    else
                    {
                        var villages = context.Villages
                            .Where(x => x.AccountId == accountId.Value)
                            .Select(x => new VillageId(x.Id))
                            .ToList();
                        foreach (var village in villages)
                        {
                            taskManager.Remove<UpdateBuildingTask.Task>(accountId, village);
                        }
                    }
                }

                if (settings.ContainsKey(AccountSettingEnums.EnableAutoStartAdventure))
                {
                    if (settings[AccountSettingEnums.EnableAutoStartAdventure] == 1)
                    {
                        if (context.IsStartAdventureEnable(accountId) && !taskManager.IsExist<StartAdventureTask.Task>(accountId))
                        {
                            taskManager.Add(new StartAdventureTask.Task(accountId));
                        }
                    }
                    else
                    {
                        taskManager.Remove<StartAdventureTask.Task>(accountId);
                    }
                }

                if (settings.ContainsKey(AccountSettingEnums.EnableAutoClaimDailyQuest))
                {
                    if (settings[AccountSettingEnums.EnableAutoClaimDailyQuest] == 1)
                    {
                        if (context.IsDailyQuestEnable(accountId) && !taskManager.IsExist<ClaimDailyQuestTask.Task>(accountId))
                        {
                            taskManager.Add(new ClaimDailyQuestTask.Task(accountId));
                        }
                    }
                    else
                    {
                        taskManager.Remove<ClaimDailyQuestTask.Task>(accountId);
                    }
                }

                if (settings.ContainsKey(AccountSettingEnums.EnableAutoSetHeroPoint))
                {
                    if (settings[AccountSettingEnums.EnableAutoSetHeroPoint] == 1)
                    {
                        if (context.IsSetHeroPointEnable(accountId) && !taskManager.IsExist<SetHeroPointTask.Task>(accountId))
                        {
                            taskManager.Add(new SetHeroPointTask.Task(accountId));
                        }
                    }
                    else
                    {
                        taskManager.Remove<SetHeroPointTask.Task>(accountId);
                    }
                }
            }
        }
    }
}