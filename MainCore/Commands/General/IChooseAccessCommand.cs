using FluentResults;
using MainCore.DTO;
using MainCore.Entities;

namespace MainCore.Commands.General
{
    public interface IChooseAccessCommand
    {
        AccessDto Value { get; }

        Task<Result> Execute(AccountId accountId, bool ignoreSleepTime);
    }
}