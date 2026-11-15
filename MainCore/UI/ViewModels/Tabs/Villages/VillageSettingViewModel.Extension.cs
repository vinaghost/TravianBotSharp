using MainCore.Features.ClaimVillageQuest;
using MainCore.Features.InstantUpgrade;
using MainCore.Features.NPC;
using MainCore.Features.TrainTroop;
using MainCore.Features.Update;

namespace MainCore.UI.ViewModels.Tabs.Villages
{
    public static class VillageSettingViewModelExtension
    {
        public static void TriggerTask(this AppDbContext context, TaskManager taskManager, AccountId accountId, VillageId villageId, Dictionary<VillageSettingEnums, int> settings)
        {
            if (settings.ContainsKey(VillageSettingEnums.CompleteImmediately))
            {
                if (settings[VillageSettingEnums.CompleteImmediately] == 1)
                {
                    if (context.IsInstantUpgradeEnable(accountId, villageId) && !taskManager.IsExist<InstantUpgradeTask.Task>(accountId, villageId))
                    {
                        taskManager.Add(new InstantUpgradeTask.Task(accountId, villageId));
                    }
                }
                else
                {
                    taskManager.Remove<InstantUpgradeTask.Task>(accountId, villageId);
                }
            }

            if (settings.ContainsKey(VillageSettingEnums.TrainTroopEnable))
            {
                if (settings[VillageSettingEnums.TrainTroopEnable] == 1)
                {
                    if (context.IsTrainTroopEnable(villageId) && !taskManager.IsExist<TrainTroopTask.Task>(accountId, villageId))
                    {
                        taskManager.Add(new TrainTroopTask.Task(accountId, villageId));
                    }
                }
                else
                {
                    taskManager.Remove<TrainTroopTask.Task>(accountId, villageId);
                }
            }

            if (settings.ContainsKey(VillageSettingEnums.AutoNPCEnable))
            {
                if (settings[VillageSettingEnums.AutoNPCEnable] == 1)
                {
                    if (context.IsNPCEnable(accountId, villageId) && !taskManager.IsExist<NPCTask.Task>(accountId, villageId))
                    {
                        taskManager.Add(new NPCTask.Task(accountId, villageId));
                    }
                }
                else
                {
                    taskManager.Remove<NPCTask.Task>(accountId, villageId);
                }
            }

            if (settings.ContainsKey(VillageSettingEnums.AutoRefreshEnable))
            {
                if (settings[VillageSettingEnums.AutoRefreshEnable] == 1)
                {
                    if (context.IsRefreshEnable(villageId) && !taskManager.IsExist<UpdateVillageTask.Task>(accountId, villageId))
                    {
                        taskManager.Add(new UpdateVillageTask.Task(accountId, villageId));
                    }
                }
                else
                {
                    taskManager.Remove<UpdateVillageTask.Task>(accountId, villageId);
                }
            }

            if (settings.ContainsKey(VillageSettingEnums.AutoClaimQuestEnable))
            {
                if (settings[VillageSettingEnums.AutoClaimQuestEnable] == 1)
                {
                    if (context.IsVillageQuestEnable(villageId) && !taskManager.IsExist<ClaimVillageQuestTask.Task>(accountId, villageId))
                    {
                        taskManager.Add(new ClaimVillageQuestTask.Task(accountId, villageId));
                    }
                }
                else
                {
                    taskManager.Remove<ClaimVillageQuestTask.Task>(accountId, villageId);
                }
            }
        }
    }
}