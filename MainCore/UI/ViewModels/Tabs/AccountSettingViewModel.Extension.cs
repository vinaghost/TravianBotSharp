using System;
using System.Collections.Generic;
using System.Text;

namespace MainCore.UI.ViewModels.Tabs
{
    public static class AccountSettingViewModelExtension
    {
        public static void TriggerTask(this AppDbContext context, ITaskManager taskManager, AccountId accountId, Dictionary<AccountSettingEnums, int> settings)
        {
            if (settings.Count == 0) return;

            if (settings.ContainsKey(AccountSettingEnums.EnableAutoLoadVillageBuilding))
            {
                if (settings[AccountSettingEnums.EnableAutoLoadVillageBuilding] == 1)
                {
                    var villages = context.Villages
                        .Where(x => x.AccountId == accountId.Value)
                        .Where(x => x.Buildings.Count < 40)
                        .Select(x => new VillageId(x.Id))
                        .ToList();

                    foreach (var village in villages)
                    {
                        var task = new UpdateBuildingTask.Task(accountId, village);
                        if (!taskManager.IsExist<UpdateBuildingTask.Task>(accountId, village))
                        {
                            taskManager.Add(task);
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
                        taskManager.Remove<CompleteImmediatelyTask.Task>(accountId, village);
                    }
                }
            }

            if (settings.ContainsKey(AccountSettingEnums.EnableAutoStartAdventure))
            {
                if (settings[AccountSettingEnums.EnableAutoStartAdventure] == 1)
                {
                    var task = new StartAdventureTask.Task(accountId);
                    if (task.CanStart(context) && !taskManager.IsExist<StartAdventureTask.Task>(accountId))
                    {
                        taskManager.Add(task);
                    }
                }
                else
                {
                    taskManager.Remove<StartAdventureTask.Task>(accountId);
                }
            }
        }
    }
}