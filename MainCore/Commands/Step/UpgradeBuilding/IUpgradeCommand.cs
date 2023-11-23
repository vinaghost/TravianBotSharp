using FluentResults;
using MainCore.Entities;

namespace MainCore.Commands.Step.UpgradeBuilding
{
    public interface IUpgradeCommand
    {
        Result Execute(AccountId accountId);
    }
}