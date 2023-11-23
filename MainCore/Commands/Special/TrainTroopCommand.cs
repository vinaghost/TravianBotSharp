using FluentResults;
using MainCore.Common.Enums;
using MainCore.Common.Errors;
using MainCore.Common.Errors.TrainTroop;
using MainCore.Entities;
using MainCore.Repositories;
using MediatR;

namespace MainCore.Commands.Special
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
            {BuildingEnums.Stable, VillageSettingEnums.StableTroop },
            {BuildingEnums.Workshop, VillageSettingEnums.WorkshopTroop },
        };

        private static readonly Dictionary<BuildingEnums, (VillageSettingEnums, VillageSettingEnums)> _amountSettings = new()
        {
            {BuildingEnums.Barracks, (VillageSettingEnums.BarrackAmountMin,VillageSettingEnums.BarrackAmountMax )},
            {BuildingEnums.Stable, (VillageSettingEnums.StableAmountMin,VillageSettingEnums.StableAmountMax ) },
            {BuildingEnums.Workshop, (VillageSettingEnums.WorkshopAmountMin,VillageSettingEnums.WorkshopAmountMax ) },
        };

        private readonly IUnitOfRepository _unitOfRepository;
        private readonly IUnitOfCommand _unitOfCommand;

        public TrainTroopCommandHandler(IUnitOfRepository unitOfRepository, IUnitOfCommand unitOfCommand)
        {
            _unitOfRepository = unitOfRepository;
            _unitOfCommand = unitOfCommand;
        }

        public async Task<Result> Handle(TrainTroopCommand request, CancellationToken cancellationToken)
        {
            var accountId = request.AccountId;
            var villageId = request.VillageId;
            var buildingType = request.BuildingType;
            return await Execute(accountId, villageId, buildingType);
        }

        public async Task<Result> Execute(AccountId accountId, VillageId villageId, BuildingEnums buildingType)
        {
            Result result;
            result = _unitOfCommand.ToDorfCommand.Execute(accountId, 2);
            if (result.IsFailed) return result.WithError(new TraceMessage(TraceMessage.Line()));
            result = await _unitOfCommand.UpdateDorfCommand.Execute(accountId, villageId);
            if (result.IsFailed) return result.WithError(new TraceMessage(TraceMessage.Line()));

            var buildingLocation = _unitOfRepository.BuildingRepository.GetBuildingLocation(villageId, buildingType);
            if (buildingLocation == default)
            {
                return Result.Fail(new MissingBuilding(buildingType));
            }
            result = _unitOfCommand.ToBuildingCommand.Execute(accountId, buildingLocation);
            if (result.IsFailed) return result.WithError(new TraceMessage(TraceMessage.Line()));

            var troopSeting = _settings[buildingType];
            var troop = (TroopEnums)_unitOfRepository.VillageSettingRepository.GetByName(villageId, troopSeting);
            var (minSetting, maxSetting) = _amountSettings[buildingType];
            var amount = _unitOfRepository.VillageSettingRepository.GetByName(villageId, minSetting, maxSetting);

            result = _unitOfCommand.GetMaximumTroopCommand.Execute(accountId, troop);
            if (result.IsFailed) return result.WithError(new TraceMessage(TraceMessage.Line()));

            var maxAmount = _unitOfCommand.GetMaximumTroopCommand.Value;

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

            result = _unitOfCommand.InputAmountTroopCommand.Execute(accountId, troop, amount);
            if (result.IsFailed) return result.WithError(new TraceMessage(TraceMessage.Line()));

            return Result.Ok();
        }
    }
}