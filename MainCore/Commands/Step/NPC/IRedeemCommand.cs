using FluentResults;
using MainCore.Entities;

namespace MainCore.Commands.Step.NPC
{
    public interface IRedeemCommand
    {
        Task<Result> Execute(AccountId accountId);
    }
}