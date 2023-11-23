using FluentResults;
using MainCore.Entities;

namespace MainCore.Commands.Update
{
    public interface IUpdateVillageListCommand
    {
        Task<Result> Execute(AccountId accountId);
    }
}