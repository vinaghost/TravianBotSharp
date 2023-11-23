using FluentResults;
using MainCore.Entities;

namespace MainCore.Commands.Navigate
{
    public interface IToBuildingCommand
    {
        Result Execute(AccountId accountId, int location);
    }
}