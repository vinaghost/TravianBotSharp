using FluentResults;
using MainCore.Entities;

namespace MainCore.Commands.Step.UpgradeBuilding
{
    public interface IAddCroplandCommand
    {
        Task<Result> Execute(AccountId accountId, VillageId villageId);
    }
}