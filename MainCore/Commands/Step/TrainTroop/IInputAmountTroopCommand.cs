using FluentResults;
using MainCore.Common.Enums;
using MainCore.Entities;

namespace MainCore.Commands.Step.TrainTroop
{
    public interface IInputAmountTroopCommand
    {
        Task<Result> Execute(AccountId accountId, TroopEnums troop, int amount);
    }
}