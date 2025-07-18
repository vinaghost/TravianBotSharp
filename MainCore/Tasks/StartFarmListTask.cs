﻿using MainCore.Commands.Features.StartFarmList;
using MainCore.Commands.NextExecute;
using MainCore.Tasks.Base;

namespace MainCore.Tasks
{
    [Handler]
    public static partial class StartFarmListTask
    {
        public sealed class Task : AccountTask
        {
            public Task(AccountId accountId) : base(accountId)
            {
            }

            protected override string TaskName => "Start farm list";
        }

        private static async ValueTask<Result> HandleAsync(
            Task task,
            ISettingService settingService,
            ToFarmListPageCommand.Handler toFarmListPageCommand,
            StartAllFarmListCommand.Handler startAllFarmListCommand,
            StartActiveFarmListCommand.Handler startActiveFarmListCommand,
            NextExecuteStartFarmListTaskCommand.Handler nextExecuteStartFarmListTaskCommand,
            CancellationToken cancellationToken)
        {
            Result result;
            result = await toFarmListPageCommand.HandleAsync(new(task.AccountId), cancellationToken);
            if (result.IsFailed) return result;

            var useStartAllButton = settingService.BooleanByName(task.AccountId, AccountSettingEnums.UseStartAllButton);
            if (useStartAllButton)
            {
                result = await startAllFarmListCommand.HandleAsync(new(), cancellationToken);
                if (result.IsFailed) return result;
            }
            else
            {
                result = await startActiveFarmListCommand.HandleAsync(new(task.AccountId), cancellationToken);
                if (result.IsFailed) return result;
            }

            await nextExecuteStartFarmListTaskCommand.HandleAsync(new(task), cancellationToken);

            return Result.Ok();
        }
    }
}