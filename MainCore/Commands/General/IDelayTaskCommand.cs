using FluentResults;
using MainCore.Entities;

namespace MainCore.Commands.General
{
    public interface IDelayTaskCommand
    {
        Task<Result> Execute(AccountId accountId);
    }
}