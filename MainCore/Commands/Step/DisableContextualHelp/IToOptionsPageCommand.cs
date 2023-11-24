using FluentResults;
using MainCore.Entities;

namespace MainCore.Commands.Step.DisableContextualHelp
{
    public interface IToOptionsPageCommand
    {
        Task<Result> Execute(AccountId accountId);
    }
}