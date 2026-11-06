using MainCore.Commands.Navigate;
using MainCore.Commands.Update;

namespace MainCore.Commands.Features.UpgradeBuilding
{
    [Handler]
    public sealed partial class ToBuildPageCommand(
        ILogger logger,
        ToDorfCommand.Handler toDorfCommand,
        ToBuildingByLocationCommand.Handler toBuildingCommand,
        SwitchTabCommand.Handler switchTabCommand)
    {
        public sealed record Command(IChromeBrowser Browser, NormalBuildPlan Plan, bool IsChecked = false) : ICommand
        {
            public string Name => $"{nameof(ToBuildPageCommand).Humanize()} [{Description}]";
            private string Description => $"{Plan}";
        }

        private async ValueTask<Result> HandleAsync(Command command, CancellationToken cancellationToken)
        {
            var (browser, plan, isChecked) = command;
            var dorf = plan.Type.IsResourceField() ? 1 : 2;

            Result result;
            result = await toDorfCommand.HandleAsync(new(browser, dorf), cancellationToken);
            if (result.IsFailed) return result;

            if (!isChecked)
            {
                result = await CheckBuildingStatus(browser, plan);
                if (result.IsFailed) return result;
            }

            result = await toBuildingCommand.HandleAsync(new(browser, plan.Location), cancellationToken);
            if (result.IsFailed) return result;

            result = await SwitchManagementTab(browser, plan, cancellationToken);
            if (result.IsFailed) return result;

            return Result.Ok();
        }

        private async Task<Result> CheckBuildingStatus(IChromeBrowser browser, NormalBuildPlan plan)
        {
            bool isBuildingExist = await BuildingLayoutParser.IsBuildingLocationExist(browser.CurrentPage, plan.Type, plan.Location);

            if (!isBuildingExist)
            {
                logger.Information("{Type} at #{Location} does not exist, need to check its prerequisites", plan.Type, plan.Location);
                return new UpgradeError(UpgradeErrorType.BuildingNotExist);
            }

            var level = await BuildingLayoutParser.GetLevel(browser.CurrentPage, plan.Type, plan.Location);
            if (level >= plan.Level)
            {
                logger.Information("{Type} at #{Location} is already at level {Level}", plan.Type, plan.Location, level);
                return new UpgradeError(UpgradeErrorType.BuildingComplete);
            }

            var isUnderConstruction = await BuildingLayoutParser.IsUnderConstruction(browser.CurrentPage, plan.Type, plan.Location);
            if (isUnderConstruction && level >= plan.Level - 2)
            {
                logger.Information("{Type} at #{Location} is under construction, need to check the ingame building queue", plan.Type, plan.Location);
                return new UpgradeError(UpgradeErrorType.BuildingInQueue);
            }

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