using FluentResults;
using MainCore.Entities;

namespace MainCore.Commands.Navigate
{
    public interface ISwitchTabCommand
    {
        Task<Result> Execute(AccountId accountId, int index);
    }
}