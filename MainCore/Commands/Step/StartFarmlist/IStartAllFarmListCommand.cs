using FluentResults;
using MainCore.Entities;

namespace MainCore.Commands.Step.StartFarmlist
{
    public interface IStartAllFarmListCommand
    {
        Task<Result> Execute(AccountId accountId);
    }
}