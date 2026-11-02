using MainCore.Commands.Features.SetHeroPoint;
using MainCore.Infrasturecture.Extensions;
using MainCore.Tasks.Base;

namespace MainCore.Tasks
{
    [Handler]
    public sealed partial class SetHeroPointTask(
        CheckLevelUpCommand.Handler checkLevelUpCommand,
        OpenAttributesPanelCommand.Handler openAttributesPanelCommand,
        SetHeroPointCommand.Handler setPointCommand
        )
    {
        public sealed class Task(AccountId accountId) : AccountTask(accountId)
        {
            protected override string TaskName => "Set hero point";

            public override bool CanStart(AppDbContext context)
            {
                var settingEnable = context.BooleanByName(AccountId, AccountSettingEnums.EnableAutoSetHeroPoint);
                if (!settingEnable) return false;

                return true;
            }
        }

        private async ValueTask<Result> HandleAsync(Task task, CancellationToken cancellationToken)
        {
            Result result;
            var canSetHeroPoint = await checkLevelUpCommand.HandleAsync(new(task.AccountId), cancellationToken);
            if (!canSetHeroPoint) return Result.Ok();

            result = await openAttributesPanelCommand.HandleAsync(new(task.AccountId), cancellationToken);
            if (result.IsFailed) return result;
            result = await setPointCommand.HandleAsync(new(task.AccountId), cancellationToken);
            if (result.IsFailed) return result;
            return Result.Ok();
        }
    }
}