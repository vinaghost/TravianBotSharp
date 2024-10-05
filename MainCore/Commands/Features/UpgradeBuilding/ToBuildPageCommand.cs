using MainCore.Commands.Abstract;
using MainCore.Common.Models;

namespace MainCore.Commands.Features.UpgradeBuilding
{
    [RegisterScoped(Registration = RegistrationStrategy.Self)]
    public class ToBuildPageCommand : CommandBase, ICommand<NormalBuildPlan>
    {
        private readonly ToBuildingCommand _toBuildingCommand;
        private readonly SwitchTabCommand _switchTabCommand;
        private readonly GetBuilding _getBuilding;

        public ToBuildPageCommand(DataService dataService, ToBuildingCommand toBuildingCommand, SwitchTabCommand switchTabCommand, GetBuilding getBuilding) : base(dataService)
        {
            _toBuildingCommand = toBuildingCommand;
            _switchTabCommand = switchTabCommand;
            _getBuilding = getBuilding;
        }

        public async Task<Result> Execute(NormalBuildPlan plan, CancellationToken cancellationToken)
        {
            var villageId = _dataService.VillageId;

            Result result;
            result = await _toBuildingCommand.Execute(plan.Location, cancellationToken);
            if (result.IsFailed) return result.WithError(TraceMessage.Error(TraceMessage.Line()));

            var building = _getBuilding.Execute(villageId, plan.Location);
            if (building.Type == BuildingEnums.Site)
            {
                var tabIndex = plan.Type.GetBuildingsCategory();
                result = await _switchTabCommand.Execute(tabIndex, cancellationToken);
                if (result.IsFailed) return result.WithError(TraceMessage.Error(TraceMessage.Line()));
            }
            else
            {
                if (building.Level < 1) return Result.Ok();
                if (!building.Type.HasMultipleTabs()) return Result.Ok();
                result = await _switchTabCommand.Execute(0, cancellationToken);
                if (result.IsFailed) return result.WithError(TraceMessage.Error(TraceMessage.Line()));
            }
            return Result.Ok();
        }
    }
}