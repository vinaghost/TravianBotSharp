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
                    var task = new InstantUpgradeTask.Task(accountId, villageId);
                    if (task.CanStart(context) && !taskManager.IsExist<InstantUpgradeTask.Task>(accountId, villageId))
                    {
                        taskManager.Add(task);
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
                    var task = new TrainTroopTask.Task(accountId, villageId);
                    if (task.CanStart(context) && !taskManager.IsExist<TrainTroopTask.Task>(accountId, villageId))
                    {
                        taskManager.Add(task);
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
                    var task = new NpcTask.Task(accountId, villageId);
                    if (task.CanStart(context) && !taskManager.IsExist<NpcTask.Task>(accountId, villageId))
                    {
                        taskManager.Add(task);
                    }
                }
                else
                {
                    taskManager.Remove<NpcTask.Task>(accountId, villageId);
                }
            }

            if (settings.ContainsKey(VillageSettingEnums.AutoRefreshEnable))
            {
                if (settings[VillageSettingEnums.AutoRefreshEnable] == 1)
                {
                    var task = new UpdateVillageTask.Task(accountId, villageId);
                    if (task.CanStart(context) && !taskManager.IsExist<UpdateVillageTask.Task>(accountId, villageId))
                    {
                        taskManager.Add(task);
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
                    var task = new ClaimVillageQuestTask.Task(accountId, villageId);
                    if (task.CanStart(context) && !taskManager.IsExist<ClaimVillageQuestTask.Task>(accountId, villageId))
                    {
                        taskManager.Add(task);
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