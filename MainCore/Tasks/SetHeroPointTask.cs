using MainCore.Commands.Features.SetHeroPoint;
using MainCore.Infrasturecture.Extensions;
using MainCore.Tasks.Base;

namespace MainCore.Tasks
{
    [Handler]
    public sealed partial class SetHeroPointTask(
        IChromeBrowser browser,
        OpenSetPointPanelCommand.Handler openSetHeroPointPanelCommand,
        SetPointCommand.Handler setPointCommand
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

        private async ValueTask<Result> HandleAsync(
#pragma warning disable S1172 // Unused method parameters should be removed
#pragma warning disable IDE0060 // Remove unused parameter
            Task task,
#pragma warning restore IDE0060 // Remove unused parameter
#pragma warning restore S1172 // Unused method parameters should be removed
            CancellationToken cancellationToken
           )
        {
            Result result;
            var canSetHeroPoint = await browser.CurrentPage.Locator("#topBarHero i.levelUp.show").CountAsync() > 0;
            if (!canSetHeroPoint) return Result.Ok();
            result = await openSetHeroPointPanelCommand.HandleAsync(new(browser), cancellationToken);
            if (result.IsFailed) return result;
            result = await setPointCommand.HandleAsync(new(browser), cancellationToken);
            if (result.IsFailed) return result;
            return Result.Ok();
        }
    }
}