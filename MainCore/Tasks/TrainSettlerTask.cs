using FluentResults;
using MainCore.Commands;
using MainCore.Commands.Base;
using MainCore.Commands.Features.Step.TrainTroop;
using MainCore.Commands.Features.Step.UpgradeBuilding;
using MainCore.Common.Enums;
using MainCore.Common.Errors;
using MainCore.Common.Errors.Storage;
using MainCore.Common.Extensions;
using MainCore.Infrasturecture.AutoRegisterDi;
using MainCore.Parsers;
using MainCore.Repositories;
using MainCore.Services;
using MainCore.Tasks.Base;
using MediatR;

namespace MainCore.Tasks
{
    [RegisterAsTransient(withoutInterface: true)]
    public class TrainSettlerTask : VillageTask
    {
        private readonly ICommandHandler<UseHeroResourceCommand> _useHeroResourceCommand;
        private readonly ITaskManager _taskManager;
        private readonly IChromeManager _chromeManager;
        private readonly UnitOfParser _unitOfParser;
        private readonly ICommandHandler<InputAmountTroopCommand> _inputAmountTroopCommand;

        public TrainSettlerTask(UnitOfCommand unitOfCommand, UnitOfRepository unitOfRepository, IMediator mediator, ICommandHandler<UseHeroResourceCommand> useHeroResourceCommand, ITaskManager taskManager, IChromeManager chromeManager, UnitOfParser unitOfParser, ICommandHandler<InputAmountTroopCommand> inputAmountTroopCommand) : base(unitOfCommand, unitOfRepository, mediator)
        {
            _useHeroResourceCommand = useHeroResourceCommand;
            _taskManager = taskManager;
            _chromeManager = chromeManager;
            _unitOfParser = unitOfParser;
            _inputAmountTroopCommand = inputAmountTroopCommand;
        }

        protected override async Task<Result> Execute()
        {
            Result result;

            if (_unitOfRepository.VillageRepository.GetSettlers(VillageId) >= 3)
            {
                return Result.Fail(new Skip("Village has enough settlers"));
            }

            result = await _unitOfCommand.ToDorfCommand.Handle(new(AccountId, 2), CancellationToken);
            if (result.IsFailed) return result.WithError(new TraceMessage(TraceMessage.Line()));
            result = await _unitOfCommand.UpdateVillageInfoCommand.Handle(new(AccountId, VillageId), CancellationToken);
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

            if (_unitOfParser.SettleParser.IsSettlerEnough(html, settler))
            {
                _unitOfRepository.VillageRepository.SetSettlers(VillageId, 3);
                return Result.Fail(new Skip("Village has enough settlers"));
            }

            var cost = settler.GetTrainCost();

            result = _unitOfRepository.StorageRepository.IsEnoughResource(VillageId, cost);
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
                    await SetEnoughResourcesTime();
                    return result
                        .WithError(new TraceMessage(TraceMessage.Line()));
                }

                var missingResource = _unitOfRepository.StorageRepository.GetMissingResource(VillageId, cost);
                var heroResourceResult = await _useHeroResourceCommand.Handle(new(AccountId, missingResource), CancellationToken);
                if (heroResourceResult.IsFailed)
                {
                    if (!heroResourceResult.HasError<Retry>())
                    {
                        await SetEnoughResourcesTime();
                    }

                    return heroResourceResult.WithError(new TraceMessage(TraceMessage.Line()));
                }
            }

            result = await _inputAmountTroopCommand.Handle(new(AccountId, settler, 1), CancellationToken);
            if (result.IsFailed) return result.WithError(new TraceMessage(TraceMessage.Line()));

            result = await _unitOfCommand.DelayClickCommand.Handle(new(AccountId), CancellationToken);
            if (result.IsFailed) return result.WithError(new TraceMessage(TraceMessage.Line()));

            result = await chromeBrowser.WaitPageLoaded(CancellationToken);
            if (result.IsFailed) return result.WithError(new TraceMessage(TraceMessage.Line()));

            html = chromeBrowser.Html;

            var currentSettler = _unitOfParser.SettleParser.GetSettlerAmount(html, settler);
            var progressSettler = _unitOfParser.SettleParser.GetProgressingSettlerAmount(html, settler);
            var totalSettler = currentSettler + progressSettler;
            _unitOfRepository.VillageRepository.SetSettlers(VillageId, totalSettler);
            if (totalSettler < 3)
            {
                ExecuteAt = DateTime.Now;
            }

            return Result.Ok();
        }

        protected override void SetName()
        {
            var village = _unitOfRepository.VillageRepository.GetVillageName(VillageId);
            _name = $"Train settler task in {village}";
        }

        private bool IsUseHeroResource()
        {
            var useHeroResource = _unitOfRepository.VillageSettingRepository.GetBooleanByName(VillageId, VillageSettingEnums.UseHeroResourceForBuilding);
            return useHeroResource;
        }

        private async Task SetEnoughResourcesTime()
        {
            ExecuteAt = DateTime.Now.AddMinutes(15);
            if (_taskManager.IsExist<UpgradeBuildingTask>(AccountId, VillageId))
            {
                var task = _taskManager.Get<UpgradeBuildingTask>(AccountId, VillageId);
                task.ExecuteAt = ExecuteAt.AddSeconds(1);
            }
            await _taskManager.ReOrder(AccountId);
        }
    }
}