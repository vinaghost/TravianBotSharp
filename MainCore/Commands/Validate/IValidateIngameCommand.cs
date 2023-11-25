using FluentResults;
using MainCore.Entities;

namespace MainCore.Commands.Validate
{
    public interface IValidateIngameCommand
    {
        bool Value { get; }

        Task<Result> Execute(AccountId accountId);
    }
}