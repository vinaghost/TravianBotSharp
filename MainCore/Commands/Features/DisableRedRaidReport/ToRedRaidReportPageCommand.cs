using MainCore.Commands.Abstract;

namespace MainCore.Commands.Features.DisableRedRaidReport
{
    public class ToRedRaidReportPageCommand : NavigationBarCommand
    {
        private const string RED_REPORT = "report/offensive?opt=AAADAA==";

        public async Task<Result> Execute(IChromeBrowser chromeBrowser, CancellationToken cancellationToken)
        {
            var currentUrl = chromeBrowser.CurrentUrl;
            Uri.TryCreate(currentUrl, UriKind.Absolute, out var url);
            var baseUrl = url.GetLeftPart(UriPartial.Authority);

            Result result;
            result = await chromeBrowser.Navigate($"{baseUrl}/{RED_REPORT}", cancellationToken);
            if (result.IsFailed) return result.WithError(TraceMessage.Error(TraceMessage.Line()));
            return Result.Ok();
        }
    }
}