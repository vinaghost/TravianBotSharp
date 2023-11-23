using FluentResults;
using MainCore.Entities;

namespace MainCore.Commands.Step.UpgradeBuilding
{
    public interface ISpecialUpgradeCommand
    {
        Task<Result> Execute(AccountId accountId);
    }
}