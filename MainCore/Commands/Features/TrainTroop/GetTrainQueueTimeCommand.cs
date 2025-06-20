using MainCore.Constraints;
using MainCore.Errors.TrainTroop;
using MainCore.Enums;
using MainCore.Parsers;
using MainCore.Services;
using MainCore.Commands.Features.TrainTroop;

namespace MainCore.Commands.Features.TrainTroop
{
    [Handler]
    public static partial class GetTrainQueueTimeCommand
    {
        public sealed record Command(AccountId AccountId, VillageId VillageId, BuildingEnums Building) : IAccountVillageCommand;

        private static async ValueTask<Result<TimeSpan>> HandleAsync(
            Command command,
            IChromeBrowser browser,
            ToDorfCommand.Handler toDorfCommand,
            UpdateBuildingCommand.Handler updateBuildingCommand,
            ToBuildingCommand.Handler toBuildingCommand,
            CancellationToken cancellationToken)
        {
            var (accountId, villageId, building) = command;

            Result result;
            result = await toDorfCommand.HandleAsync(new(accountId, 2), cancellationToken);
            if (result.IsFailed) return Result.Fail(result.Errors);

            var (_, isFailed, response, errors) = await updateBuildingCommand.HandleAsync(new(accountId, villageId), cancellationToken);
            if (isFailed) return Result.Fail(errors);

            var buildingLocation = response.Buildings
                .Where(x => x.Type == building)
                .Select(x => x.Location)
                .FirstOrDefault();

            if (buildingLocation == default)
            {
                return Result.Fail<TimeSpan>(MissingBuilding.Error(building));
            }

            result = await toBuildingCommand.HandleAsync(new(accountId, buildingLocation), cancellationToken);
            if (result.IsFailed) return Result.Fail(result.Errors);

            var html = browser.Html;
            var queueTime = TrainTroopParser.GetQueueTime(html);
            return queueTime == TimeSpan.Zero ? Result.Ok(TimeSpan.Zero) : Result.Ok(queueTime);
        }
    }
}
