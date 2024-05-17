using MainCore.Commands.Abstract;

namespace MainCore.Commands.Navigate
{
    public class ToReportPageCommand : NavigationBarCommand
    {
        private const string ALL_REPORT = "report?opt=AAABAAMAAgA=";

        public async Task<Result> Execute(IChromeBrowser chromeBrowser, CancellationToken cancellationToken)
        {
            var currentUrl = chromeBrowser.CurrentUrl;
            Uri.TryCreate(currentUrl, UriKind.Absolute, out var url);
            var baseUrl = url.GetLeftPart(UriPartial.Authority);

            Result result;
            result = await chromeBrowser.Navigate($"{baseUrl}/{ALL_REPORT}", cancellationToken);
            if (result.IsFailed) return result.WithError(TraceMessage.Error(TraceMessage.Line()));
            return Result.Ok();
        }

    }
}