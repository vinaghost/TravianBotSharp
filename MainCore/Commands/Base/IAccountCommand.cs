using FluentResults;
using MainCore.Entities;

namespace MainCore.Commands.Base
{
    public interface IAccountCommand
    {
        public Task<Result> Execute(AccountId accountId, CancellationToken cancellationToken);
    }
}