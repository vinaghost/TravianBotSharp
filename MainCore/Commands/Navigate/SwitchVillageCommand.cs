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
    public class SwitchVillageCommand : ByAccountVillageIdBase, ICommand
    {
        public SwitchVillageCommand(AccountId accountId, VillageId villageId) : base(accountId, villageId)
        {
        }
    }

    [RegisterAsTransient]
    public class SwitchVillageCommandHandler : ICommandHandler<SwitchVillageCommand>
    {
        private readonly IChromeManager _chromeManager;
        private readonly UnitOfParser _unitOfParser;

        public SwitchVillageCommandHandler(IChromeManager chromeManager, UnitOfParser unitOfParser)
        {
            _chromeManager = chromeManager;
            _unitOfParser = unitOfParser;
        }

        public async Task<Result> Handle(SwitchVillageCommand command, CancellationToken cancellationToken)
        {
            var chromeBrowser = _chromeManager.Get(command.AccountId);
            var html = chromeBrowser.Html;
            var node = _unitOfParser.VillagePanelParser.GetVillageNode(html, command.VillageId);
            if (node is null) return Skip.VillageNotFound;

            if (_unitOfParser.VillagePanelParser.IsActive(node)) return Result.Ok();

            Result result;
            result = await chromeBrowser.Click(By.XPath(node.XPath));
            if (result.IsFailed) return result.WithError(new TraceMessage(TraceMessage.Line()));

            bool villageChanged(IWebDriver driver)
            {
                var doc = new HtmlDocument();
                doc.LoadHtml(driver.PageSource);

                var villageNode = _unitOfParser.VillagePanelParser.GetVillageNode(doc, command.VillageId);
                return villageNode is not null && _unitOfParser.VillagePanelParser.IsActive(villageNode);
            };

            result = await chromeBrowser.Wait(villageChanged, cancellationToken);
            if (result.IsFailed) return result.WithError(new TraceMessage(TraceMessage.Line()));
            return Result.Ok();
        }
    }
}