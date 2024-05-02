namespace MainCore.Commands.Features.UpgradeBuilding
{
    public class ConstructCommand : ICommand
    {
        public ConstructCommand(IChromeBrowser chromeBrowser, BuildingEnums building)
        {
            Building = building;
            ChromeBrowser = chromeBrowser;
        }

        public BuildingEnums Building { get; }
        public IChromeBrowser ChromeBrowser { get; }
    }

    public class ConstructCommandHandler : ICommandHandler<ConstructCommand>
    {
        private readonly IUpgradeBuildingParser _upgradeBuildingParser;

        public ConstructCommandHandler(IUpgradeBuildingParser upgradeBuildingParser)
        {
            _upgradeBuildingParser = upgradeBuildingParser;
        }

        public async Task<Result> Handle(ConstructCommand request, CancellationToken cancellationToken)
        {
            var chromeBrowser = request.ChromeBrowser;
            var building = request.Building;
            var html = chromeBrowser.Html;

            var button = _upgradeBuildingParser.GetConstructButton(html, building);
            if (button is null) return Retry.ButtonNotFound("construct");

            var result = await chromeBrowser.Click(By.XPath(button.XPath));
            if (result.IsFailed) return result.WithError(TraceMessage.Error(TraceMessage.Line()));

            result = await chromeBrowser.WaitPageChanged("dorf", cancellationToken);
            if (result.IsFailed) return result.WithError(TraceMessage.Error(TraceMessage.Line()));
            return Result.Ok();
        }
    }
}