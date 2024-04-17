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
    public class EvadeTroopTask : VillageTask
    {
        private readonly IChromeManager _chromeManager;
        private readonly IRallypointParser _rallypointParser;

        public EvadeTroopTask(UnitOfCommand unitOfCommand, UnitOfRepository unitOfRepository, IMediator mediator, IRallypointParser rallypointParser, IChromeManager chromeManager) : base(unitOfCommand, unitOfRepository, mediator)
        {
            _rallypointParser = rallypointParser;
            _chromeManager = chromeManager;
        }

        protected override async Task<Result> Execute()
        {
            Result result;

            result = await ToRallyPoint();
            if (result.IsFailed) return result.WithError(new TraceMessage(TraceMessage.Line()));

            result = await InputTroop();
            if (result.IsFailed) return result.WithError(new TraceMessage(TraceMessage.Line()));

            result = await InputCoord();
            if (result.IsFailed) return result.WithError(new TraceMessage(TraceMessage.Line()));

            result = await Send();
            if (result.IsFailed) return result.WithError(new TraceMessage(TraceMessage.Line()));

            result = await Confirm();
            if (result.IsFailed) return result.WithError(new TraceMessage(TraceMessage.Line()));

            result = await Cancel();
            if (result.IsFailed) return result.WithError(new TraceMessage(TraceMessage.Line()));

            return Result.Ok();
        }

        private async Task<Result> ToRallyPoint()
        {
            Result result;
            result = await _unitOfCommand.ToDorfCommand.Handle(new(AccountId, 2), CancellationToken);
            if (result.IsFailed) return result.WithError(new TraceMessage(TraceMessage.Line()));

            result = await _unitOfCommand.UpdateVillageInfoCommand.Handle(new(AccountId, VillageId), CancellationToken);
            if (result.IsFailed) return result.WithError(new TraceMessage(TraceMessage.Line()));

            result = await _unitOfCommand.ToBuildingCommand.Handle(new(AccountId, 39), CancellationToken);
            if (result.IsFailed) return result.WithError(new TraceMessage(TraceMessage.Line()));

            result = await _unitOfCommand.SwitchTabCommand.Handle(new(AccountId, 2), CancellationToken);
            if (result.IsFailed) return result.WithError(new TraceMessage(TraceMessage.Line()));
            return Result.Ok();
        }

        private async Task<Result> InputTroop()
        {
            var chromeBrowser = _chromeManager.Get(AccountId);
            var html = chromeBrowser.Html;

            var inputs = _rallypointParser.GetTroopInput(html).ToList();
            Result result;
            foreach (var input in inputs)
            {
                var amount = _rallypointParser.GetTroopAmount(input);

                result = await chromeBrowser.InputTextbox(By.XPath(input.XPath), $"{amount}");
                if (result.IsFailed) return result.WithError(new TraceMessage(TraceMessage.Line()));
            }

            var raidButton = _rallypointParser.GetRaidInput(html);
            if (raidButton is null) return Result.Fail(Retry.ButtonNotFound("raid"));

            result = await chromeBrowser.Click(By.XPath(raidButton.XPath));
            if (result.IsFailed) return result.WithError(new TraceMessage(TraceMessage.Line()));

            return Result.Ok();
        }

        private async Task<Result> InputCoord()
        {
            var chromeBrowser = _chromeManager.Get(AccountId);
            var html = chromeBrowser.Html;
            var xInput = _rallypointParser.GetXInput(html);
            if (xInput is null) return Result.Fail(Retry.TextboxNotFound("x input"));

            var yInput = _rallypointParser.GetYInput(html);
            if (yInput is null) return Result.Fail(Retry.TextboxNotFound("y input"));

            var x = _unitOfRepository.AccountSettingRepository.GetByName(AccountId, Common.Enums.AccountSettingEnums.EvadeTroopX);
            var y = _unitOfRepository.AccountSettingRepository.GetByName(AccountId, Common.Enums.AccountSettingEnums.EvadeTroopY);

            Result result;
            result = await chromeBrowser.InputTextbox(By.XPath(xInput.XPath), $"{x}");
            if (result.IsFailed) return result.WithError(new TraceMessage(TraceMessage.Line()));

            result = await chromeBrowser.InputTextbox(By.XPath(yInput.XPath), $"{y}");
            if (result.IsFailed) return result.WithError(new TraceMessage(TraceMessage.Line()));

            return Result.Ok();
        }

        private async Task<Result> Send()
        {
            var chromeBrowser = _chromeManager.Get(AccountId);
            var html = chromeBrowser.Html;
            var sendButton = _rallypointParser.GetSendButton(html);
            if (sendButton is null) return Result.Fail(Retry.ButtonNotFound("send"));
            Result result;
            result = await chromeBrowser.Click(By.XPath(sendButton.XPath));
            if (result.IsFailed) return result.WithError(new TraceMessage(TraceMessage.Line()));

            result = await chromeBrowser.WaitPageLoaded(CancellationToken);
            if (result.IsFailed) return result.WithError(new TraceMessage(TraceMessage.Line()));

            return Result.Ok();
        }

        private async Task<Result> Confirm()
        {
            var chromeBrowser = _chromeManager.Get(AccountId);
            var html = chromeBrowser.Html;

            html = chromeBrowser.Html;
            Result result;
            if (_rallypointParser.IsInvalidCoordinate(html))
            {
                result = await InputDefaultCoord();
                if (result.IsFailed) return result.WithError(new TraceMessage(TraceMessage.Line()));

                result = await Send();
                if (result.IsFailed) return result.WithError(new TraceMessage(TraceMessage.Line()));
            }

            html = chromeBrowser.Html;
            var confirmButton = _rallypointParser.GetConfirmButton(html);

            result = await chromeBrowser.Click(By.XPath(confirmButton.XPath));
            if (result.IsFailed) return result.WithError(new TraceMessage(TraceMessage.Line()));

            result = await chromeBrowser.WaitPageLoaded(CancellationToken);
            if (result.IsFailed) return result.WithError(new TraceMessage(TraceMessage.Line()));

            return Result.Ok();
        }

        private async Task<Result> InputDefaultCoord()
        {
            var chromeBrowser = _chromeManager.Get(AccountId);
            var html = chromeBrowser.Html;

            var inputs = _rallypointParser.GetTroopInput(html).ToList();
            Result result;
            foreach (var input in inputs)
            {
                var amount = _rallypointParser.GetTroopAmount(input);

                result = await chromeBrowser.InputTextbox(By.XPath(input.XPath), $"{amount}");
                if (result.IsFailed) return result.WithError(new TraceMessage(TraceMessage.Line()));
            }

            var xInput = _rallypointParser.GetXInput(html);
            if (xInput is null) return Result.Fail(Retry.TextboxNotFound("x input"));

            var yInput = _rallypointParser.GetYInput(html);
            if (yInput is null) return Result.Fail(Retry.TextboxNotFound("y input"));

            result = await chromeBrowser.InputTextbox(By.XPath(xInput.XPath), "50");
            if (result.IsFailed) return result.WithError(new TraceMessage(TraceMessage.Line()));

            result = await chromeBrowser.InputTextbox(By.XPath(yInput.XPath), "50");
            if (result.IsFailed) return result.WithError(new TraceMessage(TraceMessage.Line()));

            var raidButton = _rallypointParser.GetRaidInput(html);
            if (raidButton is null) return Result.Fail(Retry.ButtonNotFound("raid"));

            result = await chromeBrowser.Click(By.XPath(raidButton.XPath));
            if (result.IsFailed) return result.WithError(new TraceMessage(TraceMessage.Line()));

            return Result.Ok();
        }

        private async Task<Result> Cancel()
        {
            await Task.Delay(TimeSpan.FromSeconds(Random.Shared.Next(50, 70)));

            var chromeBrowser = _chromeManager.Get(AccountId);
            var html = chromeBrowser.Html;

            var cancelButton = _rallypointParser.GetCancelButton(html);
            if (cancelButton is null) return Result.Fail(Retry.ButtonNotFound("cancel"));

            Result result;
            result = await chromeBrowser.Click(By.XPath(cancelButton.XPath));
            if (result.IsFailed) return result.WithError(new TraceMessage(TraceMessage.Line()));
            return Result.Ok();
        }

        protected override void SetName()
        {
            var village = _unitOfRepository.VillageRepository.GetVillageName(VillageId);
            _name = $"Evade troop in {village}";
        }
    }
}