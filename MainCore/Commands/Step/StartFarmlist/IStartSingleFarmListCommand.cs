using FluentResults;
using MainCore.Entities;

namespace MainCore.Commands.Step.StartFarmlist
{
    public interface IStartSingleFarmListCommand
    {
        Task<Result> Execute(AccountId accountId, FarmId farmlistId);
    }
}