using FluentResults;
using MainCore.Commands;
using MainCore.Common.Enums;
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

            result = await Input();
            if (result.IsFailed) return result.WithError(new TraceMessage(TraceMessage.Line()));

            result = await Contribute();
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

        private async Task<Result> Input()
        {
            var chromeBrowser = _chromeManager.Get(AccountId);

            var storage = chromeBrowser.GetStorage();

            var cannies = _unitOfRepository.BuildingRepository.GetCrannyCapacity(VillageId);

            for (int i = 0; i < 4; i++)
            {
                storage[i] = RoundUpTo100(Math.Min(storage[i] - cannies, 0));
            }

            var html = chromeBrowser.Html;
            var inputs = _allianceParser.GetBonusInputs(html).ToArray();

            Result result;
            for (var i = 0; i < 4; i++)
            {
                result = await chromeBrowser.InputTextbox(By.XPath(inputs[i].XPath), $"{storage[i]}");
                if (result.IsFailed) return result.WithError(new TraceMessage(TraceMessage.Line()));
            }

            return Result.Ok();
        }

        private async Task<Result> Contribute()
        {
            var bonus = (AllianceBonusEnums)_unitOfRepository.AccountSettingRepository.GetByName(AccountId, Common.Enums.AccountSettingEnums.DonateResourceType);
            var chromeBrowser = _chromeManager.Get(AccountId);
            var html = chromeBrowser.Html;

            var selector = _allianceParser.GetBonusSelector(html, bonus);

            Result result;
            result = await chromeBrowser.Click(By.XPath(selector.XPath));
            if (result.IsFailed) return result.WithError(new TraceMessage(TraceMessage.Line()));

            result = await _unitOfCommand.DelayClickCommand.Handle(new(AccountId), CancellationToken);
            if (result.IsFailed) return result.WithError(new TraceMessage(TraceMessage.Line()));

            var contribute = _allianceParser.GetContributeButton(html);

            result = await chromeBrowser.Click(By.XPath(contribute.XPath));
            if (result.IsFailed) return result.WithError(new TraceMessage(TraceMessage.Line()));

            return Result.Ok();
        }

        private static long RoundUpTo100(long res)
        {
            if (res == 0) return 0;
            var remainder = res % 100;
            return res + (100 - remainder);
        }

        protected override void SetName()
        {
            var village = _unitOfRepository.VillageRepository.GetVillageName(VillageId);
            _name = $"Donate resource in {village}";
        }
    }
}