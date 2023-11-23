using FluentResults;
using MainCore.Entities;

namespace MainCore.Commands.Step.UpgradeBuilding
{
    public interface IUseHeroResourceCommand
    {
        Task<Result> Execute(AccountId accountId, long[] requiredResource);
    }
}