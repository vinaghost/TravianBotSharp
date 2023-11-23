using FluentResults;
using MainCore.Entities;

namespace MainCore.Commands.Update
{
    public interface IUpdateHeroItemsCommand
    {
        Task<Result> Execute(AccountId accountId);
    }
}