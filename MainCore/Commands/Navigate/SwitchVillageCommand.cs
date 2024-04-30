using HtmlAgilityPack;
using MainCore.Commands.Base;
using MainCore.Common.MediatR;

namespace MainCore.Commands.Navigate
{
    public class SwitchVillageCommand : ByAccountVillageIdBase, ICommand
    {
        public SwitchVillageCommand(AccountId accountId, VillageId villageId) : base(accountId, villageId)
        {
        }
    }

    public class SwitchVillageCommandHandler : ICommandHandler<SwitchVillageCommand>
    {
        private readonly IChromeManager _chromeManager;
        private readonly ILogService _logService;
        private readonly IVillagePanelParser _villagePanelParser;

        public SwitchVillageCommandHandler(IChromeManager chromeManager, ILogService logService, IVillagePanelParser villagePanelParser)
        {
            _chromeManager = chromeManager;
            _logService = logService;
            _villagePanelParser = villagePanelParser;
        }

        public async Task<Result> Handle(SwitchVillageCommand command, CancellationToken cancellationToken)
        {
            var chromeBrowser = _chromeManager.Get(command.AccountId);
            var html = chromeBrowser.Html;
            var node = _villagePanelParser.GetVillageNode(html, command.VillageId);
            if (node is null) return Skip.VillageNotFound;

            if (_villagePanelParser.IsActive(node)) return Result.Ok();

            var current = _villagePanelParser.GetCurrentVillageId(html);

            Result result;
            result = await chromeBrowser.Click(By.XPath(node.XPath));
            if (result.IsFailed) return result.WithError(TraceMessage.Error(TraceMessage.Line()));

            bool villageChanged(IWebDriver driver)
            {
                var doc = new HtmlDocument();
                doc.LoadHtml(driver.PageSource);

                var villageNode = _villagePanelParser.GetVillageNode(doc, command.VillageId);
                return villageNode is not null && _villagePanelParser.IsActive(villageNode);
            };

            result = await chromeBrowser.Wait(villageChanged, cancellationToken);
            if (result.IsFailed) return result
                    .WithError(new Error($"page stuck at changing village stage [Current: {current}] [Expected: {command.VillageId}]"))
                    .WithError(TraceMessage.Error(TraceMessage.Line()));

            return Result.Ok();
        }
    }
}