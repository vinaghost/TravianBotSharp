using FluentResults;
using MainCore.Common.Enums;
using MainCore.Common.Errors;
using MainCore.Entities;
using MainCore.Infrasturecture.AutoRegisterDi;
using MainCore.Services;

namespace MainCore.Commands.Navigate.ToBuildingCommand
{
    [RegisterAsTransient(ServerEnums.TTWars)]
    public class TTWars : IToBuildingCommand
    {
        private readonly IChromeManager _chromeManager;

        public TTWars(IChromeManager chromeManager)
        {
            _chromeManager = chromeManager;
        }

        public async Task<Result> Execute(AccountId accountId, int location)
        {
            var chromeBrowser = _chromeManager.Get(accountId);
            var currentUrl = new Uri(chromeBrowser.CurrentUrl);
            var host = currentUrl.GetLeftPart(UriPartial.Authority);
            Result result;
            result = await chromeBrowser.Navigate($"{host}/build.php?id={location}");
            if (result.IsFailed) return result.WithError(new TraceMessage(TraceMessage.Line()));
            result = await chromeBrowser.WaitPageLoaded();
            if (result.IsFailed) return result.WithError(new TraceMessage(TraceMessage.Line()));
            return Result.Ok();
        }
    }
}