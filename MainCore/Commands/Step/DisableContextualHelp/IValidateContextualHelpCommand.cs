using FluentResults;
using MainCore.Entities;

namespace MainCore.Commands.Step.DisableContextualHelp
{
    public interface IValidateContextualHelpCommand
    {
        bool Value { get; }

        Task<Result> Execute(AccountId accountId);
    }
}