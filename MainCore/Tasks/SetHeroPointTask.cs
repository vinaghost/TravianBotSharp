using MainCore.Commands.Features.SetHeroPoint;

namespace MainCore.Tasks
{
    [Handler]
    public sealed partial class SetHeroPointTask(
        ChromeManager chromeManager,
        CheckLevelUpCommand.Handler checkLevelUpCommand,
        OpenAttributesPanelCommand.Handler openAttributesPanelCommand,
        SetHeroPointCommand.Handler setPointCommand
        )
    {
        public sealed class Task(AccountId accountId) : AccountTask(accountId)
        {
            protected override string TaskName => nameof(SetHeroPointTask).Humanize();

            public override bool CanStart(AppDbContext context)
            {
                var settingEnable = context.BooleanByName(AccountId, AccountSettingEnums.EnableAutoSetHeroPoint);
                if (!settingEnable) return false;

                return true;
            }
        }

        private async ValueTask<Result> HandleAsync(Task task, CancellationToken cancellationToken)
        {
            var browser = chromeManager.Get(task.AccountId);
            Result result;
            var canSetHeroPoint = await checkLevelUpCommand.HandleAsync(new(browser), cancellationToken);
            if (!canSetHeroPoint) return Result.Ok();

            result = await openAttributesPanelCommand.HandleAsync(new(browser), cancellationToken);
            if (result.IsFailed) return result;
            result = await setPointCommand.HandleAsync(new(browser), cancellationToken);
            if (result.IsFailed) return result;
            return Result.Ok();
        }
    }
}