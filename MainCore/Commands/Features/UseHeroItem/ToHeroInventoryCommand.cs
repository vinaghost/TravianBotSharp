using MainCore.Commands.Abstract;

namespace MainCore.Commands.Features.UseHeroItem
{
    [RegisterScoped(Registration = RegistrationStrategy.Self)]
    public class ToHeroInventoryCommand(DataService dataService) : CommandBase(dataService)
    {
        public override async Task<Result> Execute(CancellationToken cancellationToken)
        {
            var chromeBrowser = _dataService.ChromeBrowser;
            var html = chromeBrowser.Html;
            var avatar = InventoryParser.GetHeroAvatar(html);
            if (avatar is null) return Retry.ButtonNotFound("avatar hero");

            static bool tabActived(IWebDriver driver)
            {
                var doc = new HtmlDocument();
                doc.LoadHtml(driver.PageSource);
                return InventoryParser.IsInventoryPage(doc);
            }

            Result result;
            result = await chromeBrowser.Click(By.XPath(avatar.XPath), "hero", tabActived, cancellationToken);
            if (result.IsFailed) return result.WithError(TraceMessage.Error(TraceMessage.Line()));

            return Result.Ok();
        }
    }
}