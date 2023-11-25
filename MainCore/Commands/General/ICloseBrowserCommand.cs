using FluentResults;
using MainCore.Entities;

namespace MainCore.Commands.General
{
    public interface ICloseBrowserCommand
    {
        Result Execute(AccountId accountId);
    }
}