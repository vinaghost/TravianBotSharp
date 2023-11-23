using FluentResults;
using MainCore.Entities;

namespace MainCore.Commands.Navigate
{
    public interface ISwitchTabCommand
    {
        Result Execute(AccountId accountId, int index);
    }
}