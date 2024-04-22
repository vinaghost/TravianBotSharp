using FluentResults;
using HtmlAgilityPack;
using MainCore.Commands.Base;
using MainCore.Common.Errors;
using MainCore.Common.MediatR;
using MainCore.Entities;
using MainCore.Infrasturecture.AutoRegisterDi;
using MainCore.Parsers;
using MainCore.Services;
using OpenQA.Selenium;

namespace MainCore.Commands.Navigate
{
    public class ToDorfCommand : ByAccountIdBase, ICommand
    {
        public int Dorf { get; }
        public bool IsForceReload { get; }

        public ToDorfCommand(AccountId accountId, int dorf, bool isForceReload = false) : base(accountId)
        {
            Dorf = dorf;
            IsForceReload = isForceReload;
        }
    }

    [RegisterAsTransient]
    public class ToDorfCommandHandler : ICommandHandler<ToDorfCommand>
    {
        private readonly IChromeManager _chromeManager;
        private readonly UnitOfParser _unitOfParser;

        public ToDorfCommandHandler(IChromeManager chromeManager, UnitOfParser unitOfParser)
        {
            _chromeManager = chromeManager;
            _unitOfParser = unitOfParser;
        }

        public async Task<Result> Handle(ToDorfCommand command, CancellationToken cancellationToken)
        {
            var chromeBrowser = _chromeManager.Get(command.AccountId);
            var currentUrl = chromeBrowser.CurrentUrl;
            var currentDorf = GetCurrentDorf(currentUrl);
            var dorf = command.Dorf;
            if (dorf == 0)
            {
                if (currentDorf == 0) dorf = 1;
                else dorf = currentDorf;
            }

            if (currentDorf != 0)
            {
                if (dorf == currentDorf && !command.IsForceReload) return Result.Ok();
            }

            var html = chromeBrowser.Html;

            var button = GetButton(html, dorf);
            if (button is null) return Retry.ButtonNotFound($"dorf{dorf}");

            Result result;
            result = await chromeBrowser.Click(By.XPath(button.XPath));
            if (result.IsFailed) return result.WithError(new TraceMessage(TraceMessage.Line()));
            result = await chromeBrowser.WaitPageChanged($"dorf{dorf}", cancellationToken);
            if (result.IsFailed) return result.WithError(new TraceMessage(TraceMessage.Line()));
            result = await chromeBrowser.WaitPageLoaded(cancellationToken);
            if (result.IsFailed) return result.WithError(new TraceMessage(TraceMessage.Line()));
            return Result.Ok();
        }

        private int GetCurrentDorf(string url)
        {
            if (url.Contains($"dorf1")) return 1;
            if (url.Contains($"dorf2")) return 2;
            return 0;
        }

        private HtmlNode GetButton(HtmlDocument doc, int dorf)
        {
            return dorf switch
            {
                1 => _unitOfParser.NavigationBarParser.GetResourceButton(doc),
                2 => _unitOfParser.NavigationBarParser.GetBuildingButton(doc),
                _ => null,
            };
        }
    }
}