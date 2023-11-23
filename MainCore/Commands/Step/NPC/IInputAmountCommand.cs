using FluentResults;
using MainCore.Entities;

namespace MainCore.Commands.Step.NPC
{
    public interface IInputAmountCommand
    {
        Task<Result> Execute(AccountId accountId, VillageId villageId);
    }
}