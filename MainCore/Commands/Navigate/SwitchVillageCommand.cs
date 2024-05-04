using MainCore.Commands.Abstract;

namespace MainCore.Commands.Navigate
{
    public class SwitchVillageCommand : ByVillageIdBase, ICommand
    {
        public IChromeBrowser ChromeBrowser { get; }

        public SwitchVillageCommand(IChromeBrowser chromeBrowser, VillageId villageId) : base(villageId)
        {
            ChromeBrowser = chromeBrowser;
        }
    }

    public class SwitchVillageCommandHandler : VillagePanelCommand, ICommandHandler<SwitchVillageCommand>
    {
        public async Task<Result> Handle(SwitchVillageCommand command, CancellationToken cancellationToken)
        {
            var villageId = command.VillageId;
            var chromeBrowser = command.ChromeBrowser;
            var html = chromeBrowser.Html;
            var node = GetVillageNode(html, villageId);
            if (node is null) return Skip.VillageNotFound;

            if (IsActive(node)) return Result.Ok();

            var current = GetCurrentVillageId(html);

            Result result;
            result = await chromeBrowser.Click(By.XPath(node.XPath));
            if (result.IsFailed) return result.WithError(TraceMessage.Error(TraceMessage.Line()));

            bool villageChanged(IWebDriver driver)
            {
                var doc = new HtmlDocument();
                doc.LoadHtml(driver.PageSource);

                var villageNode = GetVillageNode(doc, villageId);
                return villageNode is not null && IsActive(villageNode);
            };

            result = await chromeBrowser.Wait(villageChanged, cancellationToken);
            if (result.IsFailed) return result
                    .WithError(new Error($"page stuck at changing village stage [Current: {current}] [Expected: {villageId}]"))
                    .WithError(TraceMessage.Error(TraceMessage.Line()));

            return Result.Ok();
        }
    }
}