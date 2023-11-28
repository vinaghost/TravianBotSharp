using FluentResults;
using MainCore.Entities;

namespace MainCore.Commands.General
{
    public interface ICloseBrowserCommand
    {
        Task<Result> Execute(AccountId accountId);
    }
}