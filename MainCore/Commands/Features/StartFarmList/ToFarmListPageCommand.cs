using MainCore.Commands.Abstract;

namespace MainCore.Commands.Features.StartFarmList
{
    [RegisterScoped(Registration = RegistrationStrategy.Self)]
    public class ToFarmListPageCommand(DataService dataService, IDbContextFactory<AppDbContext> contextFactory, SwitchVillageCommand switchVillageCommand, ToDorfCommand toDorfCommand, UpdateBuildingCommand updateBuildingCommand, ToBuildingCommand toBuildingCommand, SwitchTabCommand switchTabCommand, DelayClickCommand delayClickCommand) : CommandBase(dataService), ICommand
    {
        private readonly IDbContextFactory<AppDbContext> _contextFactory = contextFactory;
        private readonly SwitchVillageCommand _switchVillageCommand = switchVillageCommand;
        private readonly ToDorfCommand _toDorfCommand = toDorfCommand;
        private readonly UpdateBuildingCommand _updateBuildingCommand = updateBuildingCommand;
        private readonly ToBuildingCommand _toBuildingCommand = toBuildingCommand;
        private readonly SwitchTabCommand _switchTabCommand = switchTabCommand;
        private readonly DelayClickCommand _delayClickCommand = delayClickCommand;

        public async Task<Result> Execute(CancellationToken cancellationToken)
        {
            var rallypointVillageId = GetVillageHasRallypoint();
            if (rallypointVillageId == VillageId.Empty) return Skip.NoRallypoint;
            _dataService.VillageId = rallypointVillageId;

            Result result;
            result = await _switchVillageCommand.Execute(cancellationToken);
            if (result.IsFailed) return result.WithError(TraceMessage.Error(TraceMessage.Line()));

            result = await _toDorfCommand.Execute(2, cancellationToken);
            if (result.IsFailed) return result.WithError(TraceMessage.Error(TraceMessage.Line()));

            result = await _updateBuildingCommand.Execute(cancellationToken);
            if (result.IsFailed) return result.WithError(TraceMessage.Error(TraceMessage.Line()));

            result = await _toBuildingCommand.Execute(39, cancellationToken);
            if (result.IsFailed) return result.WithError(TraceMessage.Error(TraceMessage.Line()));

            result = await _switchTabCommand.Execute(4, cancellationToken);
            if (result.IsFailed) return result.WithError(TraceMessage.Error(TraceMessage.Line()));

            await _delayClickCommand.Execute(cancellationToken);
            return Result.Ok();
        }

        private VillageId GetVillageHasRallypoint()
        {
            var accountId = _dataService.AccountId;
            using var context = _contextFactory.CreateDbContext();

            var village = context.Villages
                .Where(x => x.AccountId == accountId.Value)
                .Where(x => x.Buildings.Any(x => x.Type == BuildingEnums.RallyPoint && x.Level > 0))
                .OrderByDescending(x => x.IsActive)
                .Select(x => new VillageId(x.Id))
                .FirstOrDefault();
            return village;
        }
    }
}