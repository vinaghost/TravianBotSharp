using MainCore.Commands.Base;

namespace MainCore.Commands.Navigate.ToBuildingCommandHandler
{
    [RegisterAsTransient(ServerEnums.TTWars)]
    public class TTWars : ICommandHandler<ToBuildingCommand>
    {
        private readonly IChromeManager _chromeManager;

        public TTWars(IChromeManager chromeManager)
        {
            _chromeManager = chromeManager;
        }

        public async Task<Result> Handle(ToBuildingCommand command, CancellationToken cancellationToken)
        {
            var chromeBrowser = _chromeManager.Get(command.AccountId);
            var currentUrl = new Uri(chromeBrowser.CurrentUrl);
            var host = currentUrl.GetLeftPart(UriPartial.Authority);
            Result result;
            result = await chromeBrowser.Navigate($"{host}/build.php?id={command.Location}", cancellationToken);
            if (result.IsFailed) return result.WithError(TraceMessage.Error(TraceMessage.Line()));
            result = await chromeBrowser.WaitPageLoaded(cancellationToken);
            if (result.IsFailed) return result.WithError(TraceMessage.Error(TraceMessage.Line()));
            return Result.Ok();
        }
    }
}