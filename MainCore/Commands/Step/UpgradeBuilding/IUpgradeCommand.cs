using FluentResults;
using MainCore.Entities;

namespace MainCore.Commands.Step.UpgradeBuilding
{
    public interface IUpgradeCommand
    {
        Task<Result> Execute(AccountId accountId);
    }
}