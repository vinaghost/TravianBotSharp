using FluentResults;
using MainCore.Entities;

namespace MainCore.Commands.Update
{
    public interface IUpdateAccountInfoCommand
    {
        Task<Result> Execute(AccountId accountId);
    }
}