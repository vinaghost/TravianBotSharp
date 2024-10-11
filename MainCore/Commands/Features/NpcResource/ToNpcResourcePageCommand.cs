using MainCore.Commands.Abstract;
using MainCore.Common.Errors.TrainTroop;

namespace MainCore.Commands.Features.NpcResource
{
    [RegisterScoped<ToNpcResourcePageCommand>]
    public class ToNpcResourcePageCommand : CommandBase, ICommand
    {
        private readonly ToDorfCommand _toDorfCommand;
        private readonly UpdateBuildingCommand _updateBuildingCommand;
        private readonly ToBuildingCommand _toBuildingCommand;
        private readonly SwitchTabCommand _switchTabCommand;
        private readonly GetBuildingLocation _getBuildingLocation;

        public ToNpcResourcePageCommand(IDataService dataService, ToDorfCommand toDorfCommand, UpdateBuildingCommand updateBuildingCommand, ToBuildingCommand toBuildingCommand, SwitchTabCommand switchTabCommand, GetBuildingLocation getBuildingLocation) : base(dataService)
        {
            _toDorfCommand = toDorfCommand;
            _updateBuildingCommand = updateBuildingCommand;
            _toBuildingCommand = toBuildingCommand;
            _switchTabCommand = switchTabCommand;
            _getBuildingLocation = getBuildingLocation;
        }

        public async Task<Result> Execute(CancellationToken cancellationToken)
        {
            Result result;
            result = await _toDorfCommand.Execute(2, cancellationToken);
            if (result.IsFailed) return result.WithError(TraceMessage.Error(TraceMessage.Line()));

            result = await _updateBuildingCommand.Execute(cancellationToken);
            if (result.IsFailed) return result.WithError(TraceMessage.Error(TraceMessage.Line()));

            var market = _getBuildingLocation.Execute(_dataService.VillageId, BuildingEnums.Marketplace);
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