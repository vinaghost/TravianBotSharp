using FluentResults;
using MainCore.DTO;
using MainCore.Entities;

namespace MainCore.Commands.Step.UpgradeBuilding
{
    public interface IExtractResourceFieldCommand
    {
        Task<Result> Execute(AccountId accountId, VillageId villageId, JobDto job);
    }
}