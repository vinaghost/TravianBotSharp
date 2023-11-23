using FluentResults;
using MainCore.Entities;

namespace MainCore.Commands.Step.StartFarmlist
{
    public interface IStartSingleFarmListCommand
    {
        Result Execute(AccountId accountId, FarmId farmlistId);
    }
}