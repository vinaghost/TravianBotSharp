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
using Polly;

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
        private readonly ILogService _logService;

        public SwitchVillageCommandHandler(IChromeManager chromeManager, UnitOfParser unitOfParser, ILogService logService)
        {
            _chromeManager = chromeManager;
            _unitOfParser = unitOfParser;
            _logService = logService;
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
            if (result.IsFailed) return result.WithError(TraceMessage.Error(TraceMessage.Line()));

            var logger = _logService.GetLogger(command.AccountId);
            var retryPolicy = Policy
                .HandleResult<Result>(x => x.HasError<Stop>())
                .WaitAndRetryAsync(retryCount: 3, sleepDurationProvider: _ => TimeSpan.FromSeconds(5), onRetryAsync: async (error, _, retryCount, _) =>
                {
                    html = chromeBrowser.Html;
                    var current = _unitOfParser.VillagePanelParser.GetCurrentVillageId(html);
                    logger.Warning("page stuck at changing village stage for 3mins [Current: {current}] [Expected: {expected}]", current, command.VillageId);
                    logger.Information("Retry {retryCount}", retryCount);

                    await chromeBrowser.Refresh(cancellationToken);
                });

            bool villageChanged(IWebDriver driver)
            {
                var doc = new HtmlDocument();
                doc.LoadHtml(driver.PageSource);

                var villageNode = _unitOfParser.VillagePanelParser.GetVillageNode(doc, command.VillageId);
                return villageNode is not null && _unitOfParser.VillagePanelParser.IsActive(villageNode);
            };

            var poliResult = await retryPolicy.ExecuteAndCaptureAsync(() => chromeBrowser.Wait(villageChanged, cancellationToken));

            if (poliResult.Result.IsFailed)
            {
                html = chromeBrowser.Html;
                var current = _unitOfParser.VillagePanelParser.GetCurrentVillageId(html);

                return result
                    .WithError(new Error($"page stuck at changing village stage [Current: {current}] [Expected: {command.VillageId}]"))
                    .WithError(TraceMessage.Error(TraceMessage.Line()));
            }
            return Result.Ok();
        }
    }
}