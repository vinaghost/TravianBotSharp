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

        public ToDorfCommand(AccountId accountId, int dorf) : base(accountId)
        {
            Dorf = dorf;
        }
    }

    [RegisterAsTransient]
    public class ToDorfCommandHandler : ICommandHandler<ToDorfCommand>
    {
        private readonly IChromeManager _chromeManager;
        private readonly IUnitOfParser _unitOfParser;

        public ToDorfCommandHandler(IChromeManager chromeManager, IUnitOfParser unitOfParser)
        {
            _chromeManager = chromeManager;
            _unitOfParser = unitOfParser;
        }

        public async Task<Result> Handle(ToDorfCommand command, CancellationToken cancellationToken)
        {
            var chromeBrowser = _chromeManager.Get(command.AccountId);
            var currentUrl = chromeBrowser.CurrentUrl;

            if (currentUrl.Contains($"dorf{command.Dorf}")) return Result.Ok();

            var html = chromeBrowser.Html;

            var button = GetButton(html, command.Dorf);
            if (button is null) return Retry.ButtonNotFound($"dorf{command.Dorf}");

            Result result;
            result = await chromeBrowser.Click(By.XPath(button.XPath));
            if (result.IsFailed) return result.WithError(new TraceMessage(TraceMessage.Line()));
            result = await chromeBrowser.WaitPageChanged($"dorf{command.Dorf}", cancellationToken);
            if (result.IsFailed) return result.WithError(new TraceMessage(TraceMessage.Line()));
            return Result.Ok();
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