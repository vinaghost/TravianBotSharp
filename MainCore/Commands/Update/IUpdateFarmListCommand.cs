using FluentResults;
using MainCore.Entities;

namespace MainCore.Commands.Update
{
    public interface IUpdateFarmListCommand
    {
        Task<Result> Execute(AccountId accountId);
    }
}