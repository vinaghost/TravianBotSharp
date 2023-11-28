using FluentResults;
using MainCore.Common.Models;
using MainCore.Entities;

namespace MainCore.Commands.Step.UpgradeBuilding
{
    public interface IConstructCommand
    {
        Task<Result> Execute(AccountId accountId, NormalBuildPlan plan);
    }
}