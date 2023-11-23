using FluentResults;
using MainCore.Entities;

namespace MainCore.Commands.Update
{
    public interface IUpdateDorfCommand
    {
        Task<Result> Execute(AccountId accountId, VillageId villageId);
    }
}