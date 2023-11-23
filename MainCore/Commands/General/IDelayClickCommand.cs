using FluentResults;
using MainCore.Entities;

namespace MainCore.Commands.General
{
    public interface IDelayClickCommand
    {
        Task<Result> Execute(AccountId accountId);
    }
}