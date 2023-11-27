using FluentResults;
using MainCore.Entities;

namespace MainCore.Commands.Navigate
{
    public interface IToDorfCommand
    {
        Task<Result> Execute(AccountId accountId, int dorf);
    }
}