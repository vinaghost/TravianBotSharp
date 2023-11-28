using FluentResults;
using MainCore.Entities;

namespace MainCore.Commands.Navigate
{
    public interface IToHeroInventoryCommand
    {
        Task<Result> Execute(AccountId accountId);
    }
}