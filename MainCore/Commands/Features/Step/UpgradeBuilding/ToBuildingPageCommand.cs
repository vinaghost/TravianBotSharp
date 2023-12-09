using FluentResults;
using MainCore.Commands.Base;
using MainCore.Common.Enums;
using MainCore.Common.Errors;
using MainCore.Common.MediatR;
using MainCore.Common.Models;
using MainCore.Entities;
using MainCore.Infrasturecture.AutoRegisterDi;
using MainCore.Repositories;

namespace MainCore.Commands.Features.Step.UpgradeBuilding
{
    public class ToBuildingPageCommand : ByAccountVillageIdBase, ICommand
    {
        public NormalBuildPlan Plan { get; }

        public ToBuildingPageCommand(AccountId accountId, VillageId villageId) : base(accountId, villageId)
        {
        }
    }

    [RegisterAsTransient]
    public class ToBuildingPageCommandHandler : ICommandHandler<ToBuildingPageCommand>
    {
        private readonly IUnitOfCommand _unitOfCommand;
        private readonly IUnitOfRepository _unitOfRepository;

        public ToBuildingPageCommandHandler(IUnitOfCommand unitOfCommand, IUnitOfRepository unitOfRepository)
        {
            _unitOfCommand = unitOfCommand;
            _unitOfRepository = unitOfRepository;
        }

        public async Task<Result> Handle(ToBuildingPageCommand command, CancellationToken cancellationToken)
        {
            Result result;

            var dorf = command.Plan.Location < 19 ? 1 : 2;
            result = await _unitOfCommand.ToDorfCommand.Execute(command.AccountId, dorf, cancellationToken);
            if (result.IsFailed) return result.WithError(new TraceMessage(TraceMessage.Line()));

            result = await _unitOfCommand.UpdateDorfCommand.Execute(command.AccountId, command.VillageId);
            if (result.IsFailed) return result.WithError(new TraceMessage(TraceMessage.Line()));

            result = await _unitOfCommand.ToBuildingCommand.Execute(command.AccountId, command.Plan.Location, cancellationToken);
            if (result.IsFailed) return result.WithError(new TraceMessage(TraceMessage.Line()));

            var building = _unitOfRepository.BuildingRepository.GetBuilding(command.VillageId, command.Plan.Location);
            if (building.Type == BuildingEnums.Site)
            {
                var tabIndex = GetBuildingsCategory(command.Plan.Type);
                result = await _unitOfCommand.SwitchTabCommand.Execute(command.AccountId, tabIndex, cancellationToken);
                if (result.IsFailed) return result.WithError(new TraceMessage(TraceMessage.Line()));
            }
            else
            {
                if (building.Level == 0) return Result.Ok();
                if (!HasMultipleTabs(building.Type)) return Result.Ok();
                result = await _unitOfCommand.SwitchTabCommand.Execute(command.AccountId, 0, cancellationToken);
                if (result.IsFailed) return result.WithError(new TraceMessage(TraceMessage.Line()));
            }
            return Result.Ok();
        }

        private static int GetBuildingsCategory(BuildingEnums building) => building switch
        {
            BuildingEnums.GrainMill => 2,
            BuildingEnums.Sawmill => 2,
            BuildingEnums.Brickyard => 2,
            BuildingEnums.IronFoundry => 2,
            BuildingEnums.Bakery => 2,
            BuildingEnums.Barracks => 1,
            BuildingEnums.HerosMansion => 1,
            BuildingEnums.Academy => 1,
            BuildingEnums.Smithy => 1,
            BuildingEnums.Stable => 1,
            BuildingEnums.GreatBarracks => 1,
            BuildingEnums.GreatStable => 1,
            BuildingEnums.Workshop => 1,
            BuildingEnums.TournamentSquare => 1,
            BuildingEnums.Trapper => 1,
            _ => 0,
        };

        private static bool HasMultipleTabs(BuildingEnums building) => building switch
        {
            BuildingEnums.RallyPoint => true,
            BuildingEnums.CommandCenter => true,
            BuildingEnums.Residence => true,
            BuildingEnums.Palace => true,
            BuildingEnums.Marketplace => true,
            BuildingEnums.Treasury => true,
            _ => false,
        };
    }
}