namespace MainCore.Commands.UI.Misc
{
    [Handler]
    public static partial class SaveVillageSettingCommand
    {
        public sealed record Command(AccountId AccountId, VillageId VillageId, Dictionary<VillageSettingEnums, int> Settings) : IAccountVillageCommand
        {
            public void Deconstruct(out AccountId accountId, out VillageId villageId) => (accountId, villageId) = (AccountId, VillageId);
        }

        private static async ValueTask HandleAsync(
            Command command,
            AppDbContext context,
            ITaskManager taskManager
            )
        {
            await Task.CompletedTask;
            var (accountId, villageId, settings) = command;
            if (settings.Count == 0) return;

            foreach (var setting in settings)
            {
                context.VillagesSetting
                    .Where(x => x.VillageId == villageId.Value)
                    .Where(x => x.Setting == setting.Key)
                    .ExecuteUpdate(x => x.SetProperty(x => x.Value, setting.Value));
            }

            if (settings.ContainsKey(VillageSettingEnums.Tribe))
            {
                var tribe = (TribeEnums)settings[VillageSettingEnums.Tribe];

                var wallBuilding = context.Buildings
                    .Where(x => x.VillageId == villageId.Value)
                    .Where(x => x.Location == 40)
                    .FirstOrDefault();
                var wall = tribe.GetWall();
                if (wallBuilding is not null && wallBuilding.Type != wall)
                {
                    wallBuilding.Type = wall;
                    context.SaveChanges();
                }
            }

            if (settings.ContainsKey(VillageSettingEnums.CompleteImmediately))
            {
                if (settings[VillageSettingEnums.CompleteImmediately] == 1)
                {
                    var task = new CompleteImmediatelyTask.Task(accountId, villageId);
                    if (task.CanStart(context) && !taskManager.IsExist<CompleteImmediatelyTask.Task>(accountId, villageId))
                    {
                        taskManager.Add(task);
                    }
                }
                else
                {
                    taskManager.Remove<CompleteImmediatelyTask.Task>(accountId, villageId);
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
                    var task = new ClaimQuestTask.Task(accountId, villageId);
                    if (task.CanStart(context) && !taskManager.IsExist<ClaimQuestTask.Task>(accountId, villageId))
                    {
                        taskManager.Add(task);
                    }
                }
                else
                {
                    taskManager.Remove<ClaimQuestTask.Task>(accountId, villageId);
                }
            }
        }
    }
}