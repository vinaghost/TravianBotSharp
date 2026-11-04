using MainCore.Commands.Navigate;
using MainCore.Commands.Update;

namespace MainCore.Commands.Features.UpgradeBuilding
{
    [Handler]
    public sealed partial class ToBuildPageCommand(
        ToBuildingByLocationCommand.Handler toBuildingCommand,
        SwitchTabCommand.Handler switchTabCommand)
    {
        public sealed record Command(IChromeBrowser Browser, NormalBuildPlan Plan) : ICommand
        {
            public string Name => $"{nameof(ToBuildPageCommand).Humanize()} [{Description}]";
            private string Description => $"{Plan}";
        }

        private async ValueTask<Result> HandleAsync(Command command, CancellationToken cancellationToken)
        {
            var (browser, plan) = command;
            Result result;

            result = await toBuildingCommand.HandleAsync(new(browser, plan.Location), cancellationToken);
            if (result.IsFailed) return result;

            result = await SwitchManagementTab(browser, plan, cancellationToken);
            if (result.IsFailed) return result;

            return Result.Ok();
        }

        private async ValueTask<Result> SwitchManagementTab(IChromeBrowser browser, NormalBuildPlan plan, CancellationToken cancellationToken)
        {
            Result result;
            if (IsEmptySite(browser.CurrentUrl))
            {
                var tabIndex = plan.Type.GetBuildingsCategory();

                result = await switchTabCommand.HandleAsync(new(browser, tabIndex), cancellationToken);
                if (result.IsFailed) return result;
            }
            else
            {
                result = await switchTabCommand.HandleAsync(new(browser, 0), cancellationToken);
                if (result.IsFailed) return result;
            }

            return Result.Ok();
        }

        private static bool IsEmptySite(string url) => !url.Contains("gid");
    }
}