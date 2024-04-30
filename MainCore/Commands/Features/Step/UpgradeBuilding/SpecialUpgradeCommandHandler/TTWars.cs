namespace MainCore.Commands.Features.Step.UpgradeBuilding.SpecialUpgradeCommandHandler
{
    [RegisterAsTransient(ServerEnums.TTWars)]
    public class TTWars : ICommandHandler<SpecialUpgradeCommand>
    {
        private readonly IChromeManager _chromeManager;
        private readonly UnitOfParser _unitOfParser;

        public TTWars(IChromeManager chromeManager, UnitOfParser unitOfParser)
        {
            _chromeManager = chromeManager;
            _unitOfParser = unitOfParser;
        }

        public async Task<Result> Handle(SpecialUpgradeCommand command, CancellationToken cancellationToken)
        {
            await Task.CompletedTask;
            var chromeBrowser = _chromeManager.Get(command.AccountId);
            var html = chromeBrowser.Html;
            var button = _unitOfParser.UpgradeBuildingParser.GetSpecialUpgradeButton(html);
            if (button is null) return Retry.ButtonNotFound("gold upgrade");
            var result = await chromeBrowser.Click(By.XPath(button.XPath));
            if (result.IsFailed) return result.WithError(TraceMessage.Error(TraceMessage.Line()));
            return Result.Ok();
        }
    }
}