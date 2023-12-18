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

namespace MainCore.Commands.Features.Step.NPC
{
    public class OpenNPCDialogCommand : ByAccountIdBase, ICommand
    {
        public OpenNPCDialogCommand(AccountId accountId) : base(accountId)
        {
        }
    }

    [RegisterAsTransient]
    public class OpenNPCDialogCommandHandler : ICommandHandler<OpenNPCDialogCommand>
    {
        private readonly IChromeManager _chromeManager;
        private readonly UnitOfParser _unitOfParser;

        public OpenNPCDialogCommandHandler(IChromeManager chromeManager, UnitOfParser unitOfParser)
        {
            _chromeManager = chromeManager;
            _unitOfParser = unitOfParser;
        }

        public async Task<Result> Handle(OpenNPCDialogCommand command, CancellationToken cancellationToken)
        {
            await Task.CompletedTask;
            var chromeBrowser = _chromeManager.Get(command.AccountId);
            var html = chromeBrowser.Html;

            var button = _unitOfParser.MarketParser.GetExchangeResourcesButton(html);
            if (button is null) return Result.Fail(Retry.ButtonNotFound("Exchange resources"));
            Result result;
            result = await chromeBrowser.Click(By.XPath(button.XPath));
            if (result.IsFailed) return result.WithError(new TraceMessage(TraceMessage.Line()));

            bool dialogShown(IWebDriver driver)
            {
                var doc = new HtmlDocument();
                doc.LoadHtml(driver.PageSource);

                return _unitOfParser.MarketParser.NPCDialogShown(doc);
            }

            result = await chromeBrowser.Wait(dialogShown, cancellationToken);
            if (result.IsFailed) return result.WithError(new TraceMessage(TraceMessage.Line()));

            return Result.Ok();
        }
    }
}