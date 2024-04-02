using FluentResults;
using MainCore.Commands.Base;
using MainCore.Commands.Features.Step.TrainTroop;
using MainCore.Common.Enums;
using MainCore.Common.Errors;
using MainCore.Common.Errors.TrainTroop;
using MainCore.Entities;
using MainCore.Repositories;
using MediatR;

namespace MainCore.Commands.Features
{
    public class TrainTroopCommand : IRequest<Result>
    {
        public TrainTroopCommand(AccountId accountId, VillageId villageId, BuildingEnums buildingType)
        {
            AccountId = accountId;
            VillageId = villageId;
            BuildingType = buildingType;
        }

        public AccountId AccountId { get; }
        public VillageId VillageId { get; }
        public BuildingEnums BuildingType { get; }
    }

    public class TrainTroopCommandHandler : IRequestHandler<TrainTroopCommand, Result>
    {
        private static readonly Dictionary<BuildingEnums, VillageSettingEnums> _settings = new()
        {
            {BuildingEnums.Barracks, VillageSettingEnums.BarrackTroop },
            {BuildingEnums.GreatBarracks, VillageSettingEnums.GreatBarracksTroop },
            {BuildingEnums.Stable, VillageSettingEnums.StableTroop },
            {BuildingEnums.GreatStable, VillageSettingEnums.GreatStableTroop },
            {BuildingEnums.Workshop, VillageSettingEnums.WorkshopTroop },
        };

        private static readonly Dictionary<BuildingEnums, (VillageSettingEnums, VillageSettingEnums)> _amountSettings = new()
        {
            {BuildingEnums.Barracks, (VillageSettingEnums.BarrackAmountMin,VillageSettingEnums.BarrackAmountMax )},
            {BuildingEnums.GreatBarracks, (VillageSettingEnums.GreatBarracksAmountMin,VillageSettingEnums.GreatBarracksAmountMax )},
            {BuildingEnums.Stable, (VillageSettingEnums.StableAmountMin,VillageSettingEnums.StableAmountMax ) },
            {BuildingEnums.GreatStable, (VillageSettingEnums.GreatStableAmountMin,VillageSettingEnums.GreatStableAmountMax ) },
            {BuildingEnums.Workshop, (VillageSettingEnums.WorkshopAmountMin,VillageSettingEnums.WorkshopAmountMax ) },
        };

        private readonly UnitOfRepository _unitOfRepository;
        private readonly UnitOfCommand _unitOfCommand;

        private readonly ICommandHandler<GetMaximumTroopCommand, int> _getMaximumTroopCommand;
        private readonly ICommandHandler<InputAmountTroopCommand> _inputAmountTroopCommand;

        public TrainTroopCommandHandler(UnitOfRepository unitOfRepository, UnitOfCommand unitOfCommand, ICommandHandler<GetMaximumTroopCommand, int> getMaximumTroopCommand, ICommandHandler<InputAmountTroopCommand> inputAmountTroopCommand)
        {
            _unitOfRepository = unitOfRepository;
            _unitOfCommand = unitOfCommand;
            _getMaximumTroopCommand = getMaximumTroopCommand;
            _inputAmountTroopCommand = inputAmountTroopCommand;
        }

        public async Task<Result> Handle(TrainTroopCommand request, CancellationToken cancellationToken)
        {
            var accountId = request.AccountId;
            var villageId = request.VillageId;
            var buildingType = request.BuildingType;
            return await Execute(accountId, villageId, buildingType, cancellationToken);
        }

        public async Task<Result> Execute(AccountId accountId, VillageId villageId, BuildingEnums buildingType, CancellationToken cancellationToken)
        {
            Result result;
            result = await _unitOfCommand.ToDorfCommand.Handle(new(accountId, 2), cancellationToken);
            if (result.IsFailed) return result.WithError(new TraceMessage(TraceMessage.Line()));
            result = await _unitOfCommand.UpdateVillageInfoCommand.Handle(new(accountId, villageId), cancellationToken);
            if (result.IsFailed) return result.WithError(new TraceMessage(TraceMessage.Line()));

            var buildingLocation = _unitOfRepository.BuildingRepository.GetBuildingLocation(villageId, buildingType);
            if (buildingLocation == default)
            {
                return Result.Fail(new MissingBuilding(buildingType));
            }
            result = await _unitOfCommand.ToBuildingCommand.Handle(new(accountId, buildingLocation), cancellationToken);
            if (result.IsFailed) return result.WithError(new TraceMessage(TraceMessage.Line()));

            var troopSeting = _settings[buildingType];
            var troop = (TroopEnums)_unitOfRepository.VillageSettingRepository.GetByName(villageId, troopSeting);
            var (minSetting, maxSetting) = _amountSettings[buildingType];
            var amount = _unitOfRepository.VillageSettingRepository.GetByName(villageId, minSetting, maxSetting);

            result = await _getMaximumTroopCommand.Handle(new(accountId, troop), cancellationToken);
            if (result.IsFailed) return result.WithError(new TraceMessage(TraceMessage.Line()));

            var maxAmount = _getMaximumTroopCommand.Value;

            if (maxAmount == 0)
            {
                return Result.Fail(new MissingResource(buildingType));
            }

            if (amount > maxAmount)
            {
                var trainWhenLowResource = _unitOfRepository.VillageSettingRepository.GetBooleanByName(villageId, VillageSettingEnums.TrainWhenLowResource);
                if (trainWhenLowResource)
                {
                    amount = maxAmount;
                }
                else
                {
                    return Result.Fail(new MissingResource(buildingType));
                }
            }

            result = await _inputAmountTroopCommand.Handle(new(accountId, troop, amount), cancellationToken);
            if (result.IsFailed) return result.WithError(new TraceMessage(TraceMessage.Line()));

            return Result.Ok();
        }
    }
}