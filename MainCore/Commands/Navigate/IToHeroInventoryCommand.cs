using FluentResults;
using MainCore.Entities;

namespace MainCore.Commands.Navigate
{
    public interface IToHeroInventoryCommand
    {
        Result Execute(AccountId accountId);
    }
}