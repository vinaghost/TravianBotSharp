using FluentResults;
using MainCore.Entities;

namespace MainCore.Commands.General
{
    public interface ISleepCommand
    {
        Task<Result> Execute(AccountId accountId, TimeSpan sleepTime, CancellationToken cancellationToken);
    }
}