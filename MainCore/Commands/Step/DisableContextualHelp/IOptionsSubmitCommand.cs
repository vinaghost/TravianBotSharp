using FluentResults;
using MainCore.Entities;

namespace MainCore.Commands.Step.DisableContextualHelp
{
    public interface IOptionsSubmitCommand
    {
        Task<Result> Execute(AccountId accountId);
    }
}