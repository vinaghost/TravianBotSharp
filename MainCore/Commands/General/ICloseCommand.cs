using FluentResults;
using MainCore.Entities;

namespace MainCore.Commands.General
{
    public interface ICloseCommand
    {
        Result Execute(AccountId accountId);
    }
}