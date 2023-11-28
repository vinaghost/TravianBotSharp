using FluentResults;
using MainCore.Entities;

namespace MainCore.Commands.Navigate
{
    public interface IToBuildingCommand
    {
        Task<Result> Execute(AccountId accountId, int location);
    }
}