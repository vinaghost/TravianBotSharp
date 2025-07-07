namespace MainCore.Commands.UI.Misc
{
    [Handler]
    public static partial class SaveAccountSettingCommand
    {
        public sealed record Command(AccountId AccountId, Dictionary<AccountSettingEnums, int> Settings) : IAccountCommand;

        private static async ValueTask HandleAsync(
            Command command,
            AppDbContext context,
            ITaskManager taskManager
            )
        {
            await Task.CompletedTask;
            var (accountId, settings) = command;
            if (settings.Count == 0) return;

            foreach (var setting in settings)
            {
                context.AccountsSetting
                    .Where(x => x.AccountId == accountId.Value)
                    .Where(x => x.Setting == setting.Key)
                    .ExecuteUpdate(x => x.SetProperty(x => x.Value, setting.Value));
            }

            if (settings.ContainsKey(AccountSettingEnums.EnableAutoLoadVillageBuilding))
            {
                if (settings[AccountSettingEnums.EnableAutoLoadVillageBuilding] == 1)
                {
                    var villagesSpec = new MissingBuildingVillagesSpec(accountId);
                    var villages = context.Villages
                        .WithSpecification(villagesSpec)
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
                    var villagesSpec = new VillagesSpec(accountId);
                    var villages = context.Villages
                        .WithSpecification(villagesSpec)
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