using FluentResults;
using MainCore.Entities;

namespace MainCore.Commands.Step.NPC
{
    public interface IOpenNPCDialogCommand
    {
        Task<Result> Execute(AccountId accountId);
    }
}