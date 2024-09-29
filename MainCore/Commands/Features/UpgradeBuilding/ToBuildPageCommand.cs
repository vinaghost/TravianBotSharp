using MainCore.Commands.Abstract;
using MainCore.Common.Models;

namespace MainCore.Commands.Features.UpgradeBuilding
{
    [RegisterScoped(Registration = RegistrationStrategy.Self)]
    public class ToBuildPageCommand(DataService dataService, ToBuildingCommand toBuildingCommand, SwitchTabCommand switchTabCommand) : CommandBase<NormalBuildPlan>(dataService)
    {
        private readonly ToBuildingCommand _toBuildingCommand = toBuildingCommand;
        private readonly SwitchTabCommand _switchTabCommand = switchTabCommand;

        public override async Task<Result> Execute(CancellationToken cancellationToken)
        {
            var plan = Data;
            var villageId = _dataService.VillageId;

            Result result;
            result = await _toBuildingCommand.Execute(plan.Location, cancellationToken);
            if (result.IsFailed) return result.WithError(TraceMessage.Error(TraceMessage.Line()));

            var building = new GetBuilding().Execute(villageId, plan.Location);
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