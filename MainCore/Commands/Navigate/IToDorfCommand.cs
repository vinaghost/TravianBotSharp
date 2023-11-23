using FluentResults;
using MainCore.Entities;

namespace MainCore.Commands.Navigate
{
    public interface IToDorfCommand
    {
        Result Execute(AccountId accountId, int dorf);
    }
}