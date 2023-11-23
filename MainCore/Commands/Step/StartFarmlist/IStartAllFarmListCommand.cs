using FluentResults;
using MainCore.Entities;

namespace MainCore.Commands.Step.StartFarmlist
{
    public interface IStartAllFarmListCommand
    {
        Result Execute(AccountId accountId);
    }
}