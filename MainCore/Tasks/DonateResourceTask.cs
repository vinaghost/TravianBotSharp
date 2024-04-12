using FluentResults;
using MainCore.Commands;
using MainCore.Common.Errors;
using MainCore.Infrasturecture.AutoRegisterDi;
using MainCore.Parsers;
using MainCore.Repositories;
using MainCore.Services;
using MainCore.Tasks.Base;
using MediatR;
using OpenQA.Selenium;

namespace MainCore.Tasks
{
    [RegisterAsTransient(withoutInterface: true)]
    public class DonateResourceTask : VillageTask
    {
        private readonly IChromeManager _chromeManager;
        private readonly IAllianceParser _allianceParser;

        public DonateResourceTask(UnitOfCommand unitOfCommand, UnitOfRepository unitOfRepository, IMediator mediator) : base(unitOfCommand, unitOfRepository, mediator)
        {
        }

        protected override async Task<Result> Execute()
        {
            Result result;
            result = await ToAlliancePage();
            if (result.IsFailed) return result.WithError(new TraceMessage(TraceMessage.Line()));

            result = await _unitOfCommand.SwitchTabCommand.Handle(new(AccountId, 3), CancellationToken);
            if (result.IsFailed) return result.WithError(new TraceMessage(TraceMessage.Line()));

            return Result.Ok();
        }

        private async Task<Result> ToAlliancePage()
        {
            var chromeBrowser = _chromeManager.Get(AccountId);
            var html = chromeBrowser.Html;
            var allianceButton = _allianceParser.GetAllianceButton(html);

            Result result;
            result = await chromeBrowser.Click(By.XPath(allianceButton.XPath));
            if (result.IsFailed) return result.WithError(new TraceMessage(TraceMessage.Line()));

            return Result.Ok();
        }

        protected override void SetName()
        {
            var village = _unitOfRepository.VillageRepository.GetVillageName(VillageId);
            _name = $"Donate resource in {village}";
        }
    }
}