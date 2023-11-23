using FluentResults;
using MainCore.Common.Enums;
using MainCore.Entities;

namespace MainCore.Commands.Step.TrainTroop
{
    public interface IGetMaximumTroopCommand
    {
        int Value { get; }

        Result Execute(AccountId accountId, TroopEnums troop);
    }
}