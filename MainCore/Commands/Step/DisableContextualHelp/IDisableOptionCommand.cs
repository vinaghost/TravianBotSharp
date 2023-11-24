using FluentResults;
using MainCore.Entities;

namespace MainCore.Commands.Step.DisableContextualHelp
{
    public interface IDisableOptionCommand
    {
        Task<Result> Execute(AccountId accountId);
    }
}