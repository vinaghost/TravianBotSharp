using MainCore.Commands.Abstract;
using MainCore.Common.Errors.TrainTroop;

namespace MainCore.Commands.Features.NpcResource
{
    [RegisterScoped(Registration = RegistrationStrategy.Self)]
    public class ToNpcResourcePageCommand(DataService dataService, ToDorfCommand toDorfCommand, UpdateBuildingCommand updateBuildingCommand, ToBuildingCommand toBuildingCommand, SwitchTabCommand switchTabCommand) : CommandBase(dataService)
    {
        private readonly ToDorfCommand _toDorfCommand = toDorfCommand;
        private readonly UpdateBuildingCommand _updateBuildingCommand = updateBuildingCommand;
        private readonly ToBuildingCommand _toBuildingCommand = toBuildingCommand;
        private readonly SwitchTabCommand _switchTabCommand = switchTabCommand;

        public override async Task<Result> Execute(CancellationToken cancellationToken)
        {
            Result result;
            result = await _toDorfCommand.Execute(2, cancellationToken);
            if (result.IsFailed) return result.WithError(TraceMessage.Error(TraceMessage.Line()));

            result = await _updateBuildingCommand.Execute(cancellationToken);
            if (result.IsFailed) return result.WithError(TraceMessage.Error(TraceMessage.Line()));

            var market = new GetBuildingLocation().Execute(_dataService.VillageId, BuildingEnums.Marketplace);
            if (market == default)
            {
                return MissingBuilding.Error(BuildingEnums.Marketplace);
            }

            result = await _toBuildingCommand.Execute(market, cancellationToken);
            if (result.IsFailed) return result.WithError(TraceMessage.Error(TraceMessage.Line()));

            result = await _switchTabCommand.Execute(0, cancellationToken);
            if (result.IsFailed) return result.WithError(TraceMessage.Error(TraceMessage.Line()));

            return Result.Ok();
        }
    }
}