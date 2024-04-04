using FluentResults;
using MainCore.Commands;
using MainCore.Commands.Base;
using MainCore.Commands.Features.Step.UpgradeBuilding;
using MainCore.Common.Enums;
using MainCore.Common.Errors;
using MainCore.Common.Errors.Storage;
using MainCore.Common.Extensions;
using MainCore.Infrasturecture.AutoRegisterDi;
using MainCore.Notification.Message;
using MainCore.Parsers;
using MainCore.Repositories;
using MainCore.Services;
using MainCore.Tasks.Base;
using MediatR;
using OpenQA.Selenium;

namespace MainCore.Tasks
{
    [RegisterAsTransient(withoutInterface: true)]
    public class SendSettlerTask : VillageTask
    {
        private readonly long[] Cost = new long[] { 750, 750, 750, 750 };
        private readonly IChromeManager _chromeManager;
        private readonly UnitOfParser _unitOfParser;
        private readonly ITaskManager _taskManager;
        private readonly ICommandHandler<UseHeroResourceCommand> _useHeroResourceCommand;

        public SendSettlerTask(UnitOfCommand unitOfCommand, UnitOfRepository unitOfRepository, IMediator mediator, ITaskManager taskManager, IChromeManager chromeManager, UnitOfParser unitOfParser, ICommandHandler<UseHeroResourceCommand> useHeroResourceCommand) : base(unitOfCommand, unitOfRepository, mediator)
        {
            _taskManager = taskManager;
            _chromeManager = chromeManager;
            _unitOfParser = unitOfParser;
            _useHeroResourceCommand = useHeroResourceCommand;
        }

        protected override async Task<Result> Execute()
        {
            Result result;
            if (_unitOfRepository.NewVillageRepository.IsSettling(AccountId, VillageId)) return Result.Ok();

            result = await _unitOfCommand.UpdateAccountInfoCommand.Handle(new(AccountId), CancellationToken);
            if (result.IsFailed) return result.WithError(new TraceMessage(TraceMessage.Line()));

            var enoughCP = _unitOfRepository.AccountInfoRepository.IsEnoughCP(AccountId);
            if (!enoughCP) return Result.Fail(new Skip("Not enough CP for settling"));

            var newVillage = _unitOfRepository.NewVillageRepository.Get(AccountId);
            if (newVillage is null) return Result.Fail(new Skip("There is no new village in Settle tab"));

            var progressingSettler = _unitOfRepository.VillageRepository.GetProgressingSettlers(VillageId);
            if (progressingSettler > 0)
            {
                result = await UpdateProgressingSettle();
                if (result.IsFailed) return result.WithError(new TraceMessage(TraceMessage.Line()));
            }

            var chromeBrowser = _chromeManager.Get(AccountId);
            var account = _unitOfRepository.AccountRepository.Get(AccountId);
            var (x, y) = (newVillage.X, newVillage.Y);
            var kid = 1 + ((200 - y) * (200 * 2 + 1)) + 200 + x;
            result = await chromeBrowser.Navigate($"{account.Server}build.php?gid=16&tt=2&eventType=10&targetMapId={kid}", CancellationToken);
            if (result.IsFailed) return result.WithError(new TraceMessage(TraceMessage.Line()));

            var html = chromeBrowser.Html;
            var dtoStorage = _unitOfParser.StockBarParser.Get(html);

            _unitOfRepository.StorageRepository.Update(VillageId, dtoStorage);
            await _mediator.Publish(new StorageUpdated(AccountId, VillageId), CancellationToken);

            result = _unitOfRepository.StorageRepository.IsEnoughResource(VillageId, Cost);
            if (result.IsFailed)
            {
                if (result.HasError<GranaryLimit>() || result.HasError<WarehouseLimit>())
                {
                    return result
                        .WithError(new Stop("Please take a look on building job queue"))
                        .WithError(new TraceMessage(TraceMessage.Line()));
                }

                if (!IsUseHeroResource())
                {
                    await SetEnoughResourcesTime(Cost);
                    return result
                        .WithError(new TraceMessage(TraceMessage.Line()));
                }

                var missingResource = _unitOfRepository.StorageRepository.GetMissingResource(VillageId, Cost);
                var heroResourceResult = await _useHeroResourceCommand.Handle(new(AccountId, missingResource), CancellationToken);
                if (heroResourceResult.IsFailed)
                {
                    if (!heroResourceResult.HasError<Retry>())
                    {
                        await SetEnoughResourcesTime(Cost);
                    }

                    return heroResourceResult.WithError(new TraceMessage(TraceMessage.Line()));
                }
            }
            html = chromeBrowser.Html;
            var settleButton = _unitOfParser.SettleParser.GetSettleButton(html);
            if (settleButton is null) return Result.Fail(Retry.ButtonNotFound("settle"));

            var timeArrvial = _unitOfParser.SettleParser.GetSettleArrivalTime(html);

            result = await chromeBrowser.Click(By.XPath(settleButton.XPath));
            if (result.IsFailed) return result.WithError(new TraceMessage(TraceMessage.Line()));

            result = await chromeBrowser.WaitPageLoaded(CancellationToken);
            if (result.IsFailed) return result.WithError(new TraceMessage(TraceMessage.Line()));

            _unitOfRepository.NewVillageRepository.SetVillage(newVillage.Id, VillageId);

            await _taskManager.Add<UpdateVillageTask>(AccountId, VillageId, executeTime: timeArrvial);
            return Result.Ok();
        }

        protected override void SetName()
        {
            var name = _unitOfRepository.VillageRepository.GetVillageName(VillageId);
            _name = $"Send settle in {name}";
        }

        private async Task<Result> UpdateProgressingSettle()
        {
            Result result;
            result = await _unitOfCommand.ToDorfCommand.Handle(new(AccountId, 2), CancellationToken);
            if (result.IsFailed) return result.WithError(new TraceMessage(TraceMessage.Line())); result = await _unitOfCommand.UpdateVillageInfoCommand.Handle(new(AccountId, VillageId), CancellationToken);
            if (result.IsFailed) return result.WithError(new TraceMessage(TraceMessage.Line()));

            var location = _unitOfRepository.BuildingRepository.GetSettleLocation(VillageId);
            if (location == default)
            {
                return Result.Fail(new Skip("There is no building for settle"));
            }

            result = await _unitOfCommand.ToBuildingCommand.Handle(new(AccountId, location), CancellationToken);
            if (result.IsFailed) return result.WithError(new TraceMessage(TraceMessage.Line()));

            result = await _unitOfCommand.SwitchTabCommand.Handle(new(AccountId, 1), CancellationToken);
            if (result.IsFailed) return result.WithError(new TraceMessage(TraceMessage.Line()));

            var chromeBrowser = _chromeManager.Get(AccountId);
            var html = chromeBrowser.Html;

            var tribe = (TribeEnums)_unitOfRepository.AccountSettingRepository.GetByName(AccountId, AccountSettingEnums.Tribe);
            var settler = tribe.GetSettle();

            var currentSettler = _unitOfParser.SettleParser.GetSettlerAmount(html, settler);
            var progressSettler = _unitOfParser.SettleParser.GetProgressingSettlerAmount(html, settler);
            _unitOfRepository.VillageRepository.SetSettlers(VillageId, currentSettler, progressSettler);

            if (progressSettler > 0)
            {
                var time = _unitOfParser.SettleParser.GetProgressingSettlerCompleteTime(html, settler);

                ExecuteAt = DateTime.Now.Add(time);
                if (_taskManager.IsExist<UpgradeBuildingTask>(AccountId, VillageId))
                {
                    var task = _taskManager.Get<UpgradeBuildingTask>(AccountId, VillageId);
                    task.ExecuteAt = ExecuteAt.AddSeconds(1);
                }
                await _taskManager.ReOrder(AccountId);
                return Result.Fail(new Skip("Wait training settler complete"));
            }

            return Result.Ok();
        }

        private bool IsUseHeroResource()
        {
            var useHeroResource = _unitOfRepository.VillageSettingRepository.GetBooleanByName(VillageId, VillageSettingEnums.UseHeroResourceForBuilding);
            return useHeroResource;
        }

        private async Task SetEnoughResourcesTime(long[] required)
        {
            var chromeBrowser = _chromeManager.Get(AccountId);
            var time = chromeBrowser.GetTimeEnoughResource(required);
            ExecuteAt = DateTime.Now.Add(time);
            if (_taskManager.IsExist<UpgradeBuildingTask>(AccountId, VillageId))
            {
                var task = _taskManager.Get<UpgradeBuildingTask>(AccountId, VillageId);
                task.ExecuteAt = ExecuteAt.AddSeconds(1);
            }
            await _taskManager.ReOrder(AccountId);
        }
    }
}