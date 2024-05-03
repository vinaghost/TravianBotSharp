namespace MainCore.Commands.Features.UpgradeBuilding
{
    public class UpgradeCommand : ICommand
    {
        public UpgradeCommand(IChromeBrowser chromeBrowser)
        {
            ChromeBrowser = chromeBrowser;
        }

        public IChromeBrowser ChromeBrowser { get; }
    }

    public class UpgradeCommandHandler : ICommandHandler<UpgradeCommand>
    {
        private readonly IUpgradeBuildingParser _upgradeBuildingParser;

        public UpgradeCommandHandler(IUpgradeBuildingParser upgradeBuildingParser)
        {
            _upgradeBuildingParser = upgradeBuildingParser;
        }

        public async Task<Result> Handle(UpgradeCommand request, CancellationToken cancellationToken)
        {
            var chromeBrowser = request.ChromeBrowser;
            var html = chromeBrowser.Html;

            var button = _upgradeBuildingParser.GetUpgradeButton(html);
            if (button is null) return Retry.ButtonNotFound("upgrade");

            var result = await chromeBrowser.Click(By.XPath(button.XPath));
            if (result.IsFailed) return result.WithError(TraceMessage.Error(TraceMessage.Line()));

            result = await chromeBrowser.WaitPageChanged("dorf", cancellationToken);
            if (result.IsFailed) return result.WithError(TraceMessage.Error(TraceMessage.Line()));

            return Result.Ok();
        }
    }
}