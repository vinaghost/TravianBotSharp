using FluentResults;
using MainCore.Common.Models;
using MainCore.Entities;

namespace MainCore.Commands.Step.UpgradeBuilding
{
    public interface IGetTimeWhenEnoughResourceCommand
    {
        TimeSpan Value { get; }

        Result Execute(AccountId accountId, VillageId villageId, NormalBuildPlan plan);
    }
}