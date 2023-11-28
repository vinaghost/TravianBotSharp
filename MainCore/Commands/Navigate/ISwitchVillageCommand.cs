using FluentResults;
using MainCore.Entities;

namespace MainCore.Commands.Navigate
{
    public interface ISwitchVillageCommand
    {
        Task<Result> Execute(AccountId accountId, VillageId villageId);
    }
}